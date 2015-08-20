/// <reference path="jquery-1.6.4-vsdoc.js" />

var engine;
var notation;

$(document).ready(function () {
    engine = new ChessEngine();
    notation = new Notation();
    engine.Render();
});

var ChessEngine = function () {
    var public = {};
    var canvas;
    var context;
    var blackFigures;
    var whiteFigures;
    var capturedFigures = [];
    var selectedFigure;
    var nextMoveIsWhite = true;
    var possibleMoves = [];
    var moveNumber = 0;
    var moves = [];

    var boardSize = 515;
    var boardStep = 63;

    function initEvents() {
        $("#startControl").click(function (event) {
            event.preventDefault();

            var success = true;
            while (success)
                success = undoMove();
        });

        $("#endControl").click(function (event) {
            event.preventDefault();

            var success = true;
            while (success)
                success = nextMove();
        });

        $("#previousControl").click(function (event) {
            event.preventDefault();
            undoMove();
        });

        $("#nextControl").click(function (event) {
            event.preventDefault();
            //nextMove();
            performPgnMove();
        });

        $("#playControl").click(function (event) {
            event.preventDefault();
        });
    }

    function nextMove() {
        if (!moves || moves.length === 0)
            return false;

        if (nextMoveIsWhite)
            var move = moves[(moveNumber - 1) * 2 + 2];
        else
            var move = moves[(moveNumber - 1) * 2 + 1];

        if (!move)
            return false;

        var figure = getFigure(move.oldRow, move.oldColumn);
        if (figure)
            return performMove(move, figure);
        else
            return false;
    }

    public.Render = function () {
        initEvents();

        canvas = document.getElementById("canvas");
        context = canvas.getContext("2d");

        for (var x = 0.5; x < boardSize; x += boardStep) {
            context.moveTo(x, 0.5);
            context.lineTo(x, boardSize);
        }

        for (var y = 0.5; y < boardSize; y += boardStep) {
            context.moveTo(0.5, y);
            context.lineTo(boardSize, y);
        }

        context.strokeStyle = "#eee";
        context.stroke();

        var whiteKnight1 = new figure("knight", "white", 1, 7);
        var whiteKnight2 = new figure("knight", "white", 6, 7);
        var whiteRook1 = new figure("rook", "white", 0, 7);
        var whiteRook2 = new figure("rook", "white", 7, 7);
        var whiteBishop1 = new figure("bishop", "white", 2, 7);
        var whiteBishop2 = new figure("bishop", "white", 5, 7);
        var whiteQueen = new figure("queen", "white", 3, 7);
        var whiteKing = new figure("king", "white", 4, 7);

        whiteFigures = [whiteKnight1, whiteKnight2, whiteRook1, whiteRook2, whiteBishop1, whiteBishop2, whiteQueen, whiteKing];

        for (var i = 0; i < 8; i++) {
            whiteFigures.push(new figure("pawn", "white", i, 6));
        }

        $.each(whiteFigures, function (index, fig) {
            renderFigure(fig);
        });

        var blackKnight1 = new figure("knight", "black", 1, 0);
        var blackKnight2 = new figure("knight", "black", 6, 0);
        var blackRook1 = new figure("rook", "black", 0, 0);
        var blackRook2 = new figure("rook", "black", 7, 0);
        var blackBishop1 = new figure("bishop", "black", 2, 0);
        var blackBishop2 = new figure("bishop", "black", 5, 0);
        var blackQueen = new figure("queen", "black", 3, 0);
        var blackKing = new figure("king", "black", 4, 0);

        blackFigures = [blackKnight1, blackKnight2, blackRook1, blackRook2, blackBishop1, blackBishop2, blackQueen, blackKing];

        for (var i = 0; i < 8; i++) {
            blackFigures.push(new figure("pawn", "black", i, 1));
        }

        $.each(blackFigures, function (index, fig) {
            renderFigure(fig);
        });

        canvas.addEventListener("click", canvasClick, false);

        loadPgn();
    };

    function normalize(text) {
        if (text[1] === "x")
            text = text[0] + text.slice(2);

        if (text[text.length - 1] === "+")
            text = text.slice(0, text.length - 1);

        return text;
    }

    var pgnMoves = [];

    function loadPgn() {
        $.ajax("/Service", {
            success: function (json) {
                pgnMoves = json.Moves;
            },
            error: function (error) {
                alert(error);
            }
        });
    }

    function performPgnMove() {
        var item = pgnMoves.shift();
        if (!item)
            return;

        var tokens = item.Move.split(" ");
        var whiteMoveText = tokens[0];
        var blackMoveText = tokens[1];

        var movs = [];

        whiteMoveText = normalize(whiteMoveText);
        blackMoveText = normalize(blackMoveText);

        if (whiteMoveText.length === 2)
            movs.push(pawnMove(whiteMoveText, true));

        else if (whiteMoveText.length === 3 && whiteMoveText[0] === "N")
            movs.push(knightMove(whiteMoveText, true));

        else if (whiteMoveText.length === 4 && whiteMoveText[0] === "N")
            movs.push(knightMove(whiteMoveText, true, true));

        else if (whiteMoveText.length === 3 && whiteMoveText[0] === "B")
            movs.push(bishopMove(whiteMoveText, true));

        else if (whiteMoveText.length === 3 && whiteMoveText[0] === "R")
            movs.push(rookMove(whiteMoveText, true));

        else if (whiteMoveText === "O-O" || whiteMoveText === "O-O-O")
            movs.push(castleMove(whiteMoveText, true));

        else if (whiteMoveText.length === 3 && whiteMoveText[0] === "Q")
            movs.push(queenMove(whiteMoveText, true));

        else if (whiteMoveText.length === 3)
            movs.push(pawnMove(whiteMoveText, true, true));


        if (blackMoveText.length === 2)
            movs.push(pawnMove(blackMoveText, false));

        else if (blackMoveText.length === 3 && blackMoveText[0] === "N")
            movs.push(knightMove(blackMoveText, false));

        else if (blackMoveText.length === 4 && blackMoveText[0] === "N")
            movs.push(knightMove(blackMoveText, false, true));

        else if (blackMoveText.length === 3 && blackMoveText[0] === "B")
            movs.push(bishopMove(blackMoveText, false));

        else if (blackMoveText.length === 3 && blackMoveText[0] === "R")
            movs.push(rookMove(blackMoveText, false));

        else if (blackMoveText === "O-O" || blackMoveText === "O-O-O")
            movs.push(castleMove(blackMoveText, false));

        else if (blackMoveText.length === 3 && blackMoveText[0] === "Q")
            movs.push(queenMove(blackMoveText, false));

        else if (blackMoveText.length === 3)
            movs.push(pawnMove(blackMoveText, false, true));

        $.each(movs, function (index, move) {
            var figure = getFigure(move.oldRow, move.oldColumn);
            var figureToBeCaptured = getFigure(move.row, move.column);
            if (figureToBeCaptured) {
                move.isCapture = true;
                move.capturedPiece = figureToBeCaptured;
            }
            performMove(move, figure);
        });
    }

    function pawnMove(moveText, isWhite, useRank) {

        if (useRank) {
            var row = 8 - parseInt(moveText[2]);
            var column = moveText[1].charCodeAt(0) - 97;
            var rankColumn = moveText[0].charCodeAt(0) - 97;
        }
        else {
            var row = 8 - parseInt(moveText[1]);
            var column = moveText[0].charCodeAt(0) - 97;
        }

        var chosenFigure;
        var pawns = getFiguresByType('pawn', isWhite);
        $.each(pawns, function (index, pawn) {
            if ((!useRank && pawn.column == column) ||
                (useRank &&
                            (pawn.row === row - 1 || pawn.row === row + 1)
                         &&
                            (pawn.column === rankColumn))) {
                chosenFigure = pawn;
                return;
            }
        });

        if (!chosenFigure) {
            log("could not find pawn " + moveText + (isWhite ? " white" : " black"));
            return;
        }

        return new Move(chosenFigure.row, chosenFigure.column, row, column);
    }

    function getFiguresByType(type, isWhite) {
        var figures = isWhite ? whiteFigures : blackFigures;

        return $.grep(figures, function (figure, index) {
            return figure.type === type;
        });
    }

    function getKing(isWhite) {
        var figures = isWhite ? whiteFigures : blackFigures;

        var king;
        $.each(figures, function (index, figure) {
            if (figure.type === "king") {
                king = figure;
                return;
            }
        });

        return king;
    }

    function knightMove(moveText, isWhite, useRank) {

        if (useRank) {
            var row = 8 - parseInt(moveText[3]);
            var column = moveText[2].charCodeAt(0) - 97;
            var rankColumn = moveText[1].charCodeAt(0) - 97;
        }
        else {
            var row = 8 - parseInt(moveText[2]);
            var column = moveText[1].charCodeAt(0) - 97;
        }

        var chosenFigure;
        var knights = getFiguresByType('knight', isWhite);
        $.each(knights, function (index, figure) {
            if ((figure.row + 1 === row && figure.column + 2 === column) ||
                 (figure.row + 1 === row && figure.column - 2 === column) ||
                 (figure.row - 1 === row && figure.column + 2 === column) ||
                 (figure.row - 1 === row && figure.column - 2 === column) ||
                 (figure.row + 2 === row && figure.column + 1 === column) ||
                 (figure.row + 2 === row && figure.column - 1 === column) ||
                 (figure.row - 2 === row && figure.column + 1 === column) ||
                 (figure.row - 2 === row && figure.column - 1 === column)) {
                if (!useRank || (useRank && rankColumn === figure.column)) {
                    chosenFigure = figure;
                    return;
                }
            }
        });

        if (!chosenFigure) {
            log("could not find knight " + moveText + (isWhite ? " white" : " black"));
            return;
        }

        return new Move(chosenFigure.row, chosenFigure.column, row, column);
    }

    function bishopMove(moveText, isWhite) {
        var row = 8 - parseInt(moveText[2]);
        var column = moveText[1].charCodeAt(0) - 97;

        var figure;
        var bishops = getFiguresByType('bishop', isWhite);
        $.each(bishops, function (index, bishop) {
            if ((bishop.row + bishop.column) % 2 === (row + column) % 2) {
                figure = bishop;
                return;
            }
        });

        if (!figure) {
            log("could not find bishop " + moveText + (isWhite ? " white" : " black"));
            return;
        }

        return new Move(figure.row, figure.column, row, column);
    }

    function rookMove(moveText, isWhite) {
        var row = 8 - parseInt(moveText[2]);
        var column = moveText[1].charCodeAt(0) - 97;

        var figure;
        var rooks = getFiguresByType('rook', isWhite);
        $.each(rooks, function (index, rook) {
            var obstructed = false;
            if (rook.column === column || rook.row === row) {
                if (rook.column === column) {
                    for (var i = Math.min(rook.row, row) + 1; i < Math.max(rook.row, row); i++)
                        if (getFigure(i, column))
                            obstructed = true;
                }
                else {
                    for (var i = Math.min(rook.column, column) + 1; i < Math.max(rook.column, column); i++)
                        if (getFigure(row, i))
                            obstructed = true;
                }

                if (!obstructed) {
                    figure = rook;
                    return;
                }
            }
        });

        if (!figure) {
            log("could not find rook " + moveText + (isWhite ? " white" : " black"));
            return;
        }

        return new Move(figure.row, figure.column, row, column);
    }

    function queenMove(moveText, isWhite) {
        var row = 8 - parseInt(moveText[2]);
        var column = moveText[1].charCodeAt(0) - 97;

        var figure;
        var queens = getFiguresByType('queen', isWhite);
        $.each(queens, function (index, queen) {
            figure = queen;
            return;
        });

        if (!figure) {
            log("could not find queen " + moveText + (isWhite ? " white" : " black"));
            return;
        }

        return new Move(figure.row, figure.column, row, column);
    }

    function castleMove(moveText, isWhite) {
        var row = 8 - parseInt(moveText[2]);
        var column = moveText[1].charCodeAt(0) - 97;

        var isLargeCastle = moveText === "O-O-O";

        var king = getKing(isWhite);
        var rook;
        if (!isLargeCastle) {
            if (isWhite)
                rook = getFigure(7, 7);
            else
                rook = getFigure(0, 7);
        }
        else {
            if (isWhite)
                rook = getFigure(7, 0);
            else
                rook = getFigure(0, 0);
        }

        if (!rook) {
            log("could not find rook " + moveText + (isWhite ? " white" : " black"));
            return;
        }

        if (!king) {
            log("could not find king " + moveText + (isWhite ? " white" : " black"));
            return;
        }

        var newColumn = isLargeCastle ? king.column - 2 : king.column + 2;
        var move = new Move(king.row, king.column, king.row, newColumn);
        move.castlingRook = rook;
        move.isCastle = true;
        return move;
    }

    function canvasClick(e) {
        var cell = getCursorPosition(e);
        var handled;
        $.each(blackFigures, function (index, fig) {
            if (fig.column == cell.column && fig.row == cell.row) {
                pieceClicked(fig);
                handled = true;
                return false;
            }
        });

        $.each(whiteFigures, function (index, fig) {
            if (fig.column == cell.column && fig.row == cell.row) {
                pieceClicked(fig);
                handled = true;
                return false;
            }
        });

        if (!handled)
            emptyCellClicked(cell.row, cell.column);
    }

    function undoMove() {
        if (!moves || moves.length === 0)
            return false;

        if (nextMoveIsWhite)
            var move = moves[(moveNumber - 1) * 2 + 1];
        else
            var move = moves[(moveNumber - 1) * 2];

        if (!move)
            return false;

        var figure = getFigure(move.row, move.column);
        if (!figure)
            return false;

        removeSelectionNoDraw(figure.row, figure.column);
        figure.row = move.oldRow;
        figure.column = move.oldColumn;

        renderFigure(figure);

        if (move.isCastle) {
            var rook = move.castlingRook;

            removeSelectionNoDraw(rook.row, rook.column);

            if (rook.column === 5)
                rook.column = 7;
            else if (rook.column === 3)
                rook.column = 0;
            else {
                log("cannot perform uncastle, rook is in invalid position: " + rook.row + "-" + rook.column);
                return;
            }

            renderFigure(rook);
        }

        if (move.isCapture) {
            var figureToRestore = move.capturedPiece;
            var allFigures = (figureToRestore.color === "black") ? blackFigures : whiteFigures;
            allFigures.push(figureToRestore);
            renderFigure(figureToRestore);
        }

        nextMoveIsWhite = !nextMoveIsWhite;

        notation.UndoMove(moveNumber, figure.color);

        if (figure.color === "white")
            moveNumber--;

        return true;
    }

    function performMove(move, figure) {
        log("performing move from {0}{1} to {2}{3}".format(move.oldRow, move.oldColumn, move.row, move.column));

        removeSelectionNoDraw(figure.row, figure.column);
        removeSelectionNoDraw(move.row, move.column);
        removePossibleMoves();

        if (move.isCapture) {
            var figureToRemove = getFigure(move.row, move.column);
            var allFigures = (figureToRemove.color === "black") ? blackFigures : whiteFigures;
            var idx = allFigures.indexOf(figureToRemove);
            allFigures.splice(idx, 1);
            capturedFigures.push(figureToRemove);
        }

        if (move.isCastle) {

            var rook = move.castlingRook;
            removeSelectionNoDraw(rook.row, rook.column);

            if (rook.column === 7)
                rook.column = 5;
            else if (rook.column === 0)
                rook.column = 3;
            else {
                log("cannot perform castle, rook is in invalid position: " + rook.row + "-" + rook.column);
                return;
            }

            renderFigure(rook);
        }

        figure.column = move.column;
        figure.row = move.row;
        renderFigure(figure);
        if (figure.type === 'king')
            figure.hasMoved = true;

        nextMoveIsWhite = !nextMoveIsWhite;

        moves.push(move);

        if (!nextMoveIsWhite)
            moveNumber++;

        notation.PerformMove(move, figure, nextMoveIsWhite, moveNumber);

        return true;
    }

    function emptyCellClicked(row, column) {
        log('empty cell clicked ' + row + " - " + column);

        if (selectedFigure) {
            selectedFigure.selected = false;
            removeSelection(selectedFigure.row, selectedFigure.column);
            selectedFigure = "";
        }

        var found;

        $.each(possibleMoves, function (index, move) {
            if (move.row === row && move.column === column) {

                var figure = getFigure(move.oldRow, move.oldColumn);
                if (figure) {
                    performMove(move, figure);
                }

                found = true;
                return false;
            }
        });

        if (!found)
            removePossibleMoves();
    }

    function pieceClicked(figure) {

        log("piece clicked " + figure.row + " - " + figure.column);

        if (selectedFigure && selectedFigure === figure) {
            selectedFigure.selected = false;
            removePossibleMoves();
            removeSelection(selectedFigure.row, selectedFigure.column);
            selectedFigure = "";
            return;
        }

        if (selectedFigure && figure.color !== selectedFigure.color) {

            var shouldCapture;
            $.each(possibleMoves, function (index, move) {
                if (move.row === figure.row && move.column === figure.column) {
                    shouldCapture = true;
                    return;
                }
            });

            if (shouldCapture) {
                removePossibleMoves();
                var move = new Move(selectedFigure.row, selectedFigure.column, figure.row, figure.column);
                move.isCapture = true;
                performMove(move, selectedFigure);
                selectedFigure.selected = false;
                selectedFigure = "";
            }

            return;
        }

        if (nextMoveIsWhite && figure.color === 'black')
            return;

        if (!nextMoveIsWhite && figure.color === 'white')
            return;

        removePossibleMoves();

        if (selectedFigure) {
            selectedFigure.selected = false;
            removeSelection(selectedFigure.row, selectedFigure.column);
        }
        else {
            removeSelection(figure.row, figure.column);
        }

        figure.selected = true;
        selectCell(figure);
        selectedFigure = figure;

        var movesFunction = figure.type + "Movements";
        executeFunctionByName(movesFunction, engine, figure);

        $.each(possibleMoves, function (index, move) {
            colorPossibleMove(move);
        });
    }

    function removePossibleMoves() {
        $.each(possibleMoves, function (index, move) {
            removeSelection(move.row, move.column);
        });
        possibleMoves = [];
    }

    public.queenMovements = function (figure) {
        public.rookMovements(figure);
        public.bishopMovements(figure);
    };

    public.pawnMovements = function (figure) {
        if (figure.color === 'white') {
            if (figure.row == 6) {
                pushPossibleMove(figure, -1, 0);

                var possibleFigureObstructing = getFigure(figure.row - 1, figure.column);
                if (!possibleFigureObstructing)
                    pushPossibleMove(figure, -2, 0);
            }
            else {
                pushPossibleMove(figure, -1, 0);
            }

            var possibleEnemyToTheRight = getFigure(figure.row - 1, figure.column + 1);
            if (possibleEnemyToTheRight && possibleEnemyToTheRight.color !== figure.color)
                pushPossibleMove(figure, -1, 1);

            var possibleEnemyToTheLeft = getFigure(figure.row - 1, figure.column - 1);
            if (possibleEnemyToTheLeft && possibleEnemyToTheLeft.color !== figure.color)
                pushPossibleMove(figure, -1, -1);
        }
        else {
            if (figure.row == 1) {
                pushPossibleMove(figure, 1, 0);

                var possibleFigureObstructing = getFigure(figure.row + 1, figure.column);
                if (!possibleFigureObstructing)
                    pushPossibleMove(figure, 2, 0);
            }
            else {
                pushPossibleMove(figure, 1, 0);
            }

            var possibleEnemyToTheRight = getFigure(figure.row + 1, figure.column + 1);
            if (possibleEnemyToTheRight && possibleEnemyToTheRight.color !== figure.color)
                pushPossibleMove(figure, 1, 1);

            var possibleEnemyToTheLeft = getFigure(figure.row + 1, figure.column - 1);
            if (possibleEnemyToTheLeft && possibleEnemyToTheLeft.color !== figure.color)
                pushPossibleMove(figure, 1, -1);
        }
    };

    public.knightMovements = function (figure) {
        pushPossibleMove(figure, 1, 2);
        pushPossibleMove(figure, 1, -2);
        pushPossibleMove(figure, -1, 2);
        pushPossibleMove(figure, -1, -2);
        pushPossibleMove(figure, 2, 1);
        pushPossibleMove(figure, 2, -1);
        pushPossibleMove(figure, -2, 1);
        pushPossibleMove(figure, -2, -1);
    };

    public.kingMovements = function (figure) {
        pushPossibleMove(figure, 0, 1);
        pushPossibleMove(figure, 0, -1);
        pushPossibleMove(figure, 1, 0);
        pushPossibleMove(figure, 1, -1);
        pushPossibleMove(figure, 1, 1);
        pushPossibleMove(figure, -1, 0);
        pushPossibleMove(figure, -1, -1);
        pushPossibleMove(figure, -1, 1);

        if (!figure.hasMoved) {
            if ((figure.row === 7 || figure.row === 0) && figure.column === 4) {
                var rightRook = getFigure(figure.row, 7);
                if (rightRook && rightRook.type === 'rook')
                    pushPossibleMove(figure, 0, 2);

                var leftRook = getFigure(figure.row, 0);
                if (leftRook && leftRook.type === 'rook')
                    pushPossibleMove(figure, 0, -2);
            }
        }
    };

    public.bishopMovements = function (figure) {
        if (figure.row > 0) {
            for (var i = figure.row - 1; i >= 0; i--) {
                var r = i;
                var c = figure.column - figure.row + i;
                if (!pushPossibleMove(figure, r - figure.row, c - figure.column))
                    break;
            }
        }

        if (figure.row < 7) {
            for (var i = figure.row + 1; i <= 7; i++) {
                var r = i;
                var c = figure.column - figure.row + i;
                if (!pushPossibleMove(figure, r - figure.row, c - figure.column))
                    break;
            }
        }

        if (figure.column < 7) {
            for (var i = figure.column + 1; i <= 7; i++) {
                var c = i;
                var r = figure.row + figure.column - i;
                if (!pushPossibleMove(figure, r - figure.row, c - figure.column))
                    break;
            }
        }

        if (figure.column > 0) {
            for (var i = figure.column - 1; i >= 0; i--) {
                var c = i;
                var r = figure.row + figure.column - i;
                if (!pushPossibleMove(figure, r - figure.row, c - figure.column))
                    break;
            }
        }
    };

    public.rookMovements = function (figure) {
        if (figure.row > 0) {
            for (var i = figure.row - 1; i >= 0; i--) {
                if (!pushPossibleMove(figure, i - figure.row, 0))
                    break;
            }
        }

        if (figure.row < 7) {
            for (var i = figure.row + 1; i <= 7; i++) {
                if (!pushPossibleMove(figure, i - figure.row, 0))
                    break;
            }
        }

        if (figure.column < 7) {
            for (var i = figure.column + 1; i <= 7; i++) {
                if (!pushPossibleMove(figure, 0, i - figure.column))
                    break;
            }
        }

        if (figure.column > 0) {
            for (var i = figure.column - 1; i >= 0; i--) {
                if (!pushPossibleMove(figure, 0, i - figure.column))
                    break;
            }
        }
    };

    function pushPossibleMove(figure, rowOffset, columnOffset) {

        var move = new Move(figure.row, figure.column, figure.row + rowOffset, figure.column + columnOffset);

        if (move.row > 7 || move.column > 7 || move.row < 0 || move.column < 0)
            return false;

        var isValidMove;

        var offendingFigure = getFigure(move.row, move.column);
        if (!offendingFigure) {
            isValidMove = true;
        }
        else if (offendingFigure.color !== figure.color) {
            if (figure.type === "pawn" && figure.column === offendingFigure.column) {
                // do nothing
            }
            else {
                move.isCapture = true;
                isValidMove = true;
            }
        }

        if (isValidMove)
            possibleMoves.push(move);

        return !offendingFigure;
    }

    function colorPossibleMove(move) {
        context.fillStyle = "#B4CDCD";
        context.fillRect((move.column * boardStep) + 1, (move.row * boardStep) + 1, boardStep - 1, boardStep - 1);

        if (move.isCapture)
            renderFigure(getFigure(move.row, move.column));
    }

    function selectCell(figure) {
        context.fillStyle = "#FF4500";
        context.fillRect((figure.column * boardStep) + 1, (figure.row * boardStep) + 1, boardStep - 1, boardStep - 1);
        renderFigure(figure);
    }

    function removeSelection(row, column) {
        context.fillStyle = "#FFFFFF";
        context.fillRect((column * boardStep) + 1, (row * boardStep) + 1, boardStep - 1, boardStep - 1);

        var figure = getFigure(row, column);
        if (figure)
            renderFigure(figure);
    }

    function removeSelectionNoDraw(row, column) {
        context.fillStyle = "#FFFFFF";
        context.fillRect((column * boardStep) + 1, (row * boardStep) + 1, boardStep - 1, boardStep - 1);
    }

    function renderFigure(figure) {
        if (!figure.imageObj) {
            figure.imageObj = new Image();
            figure.imageObj.src = figure.image;

            figure.imageObj.onload = function () {
                context.drawImage(figure.imageObj, figure.column * boardStep, figure.row * boardStep, boardStep, boardStep);
            };
        }
        else {
            context.drawImage(figure.imageObj, figure.column * boardStep, figure.row * boardStep, boardStep, boardStep);
        }
    }

    function figure(type, color, column, row) {
        this.type = type;
        this.image = "static/images/figures/" + color + "-" + type + ".png";
        this.column = column;
        this.row = row;
        this.color = color;
        this.selected = false;
    }

    function Cell(row, column) {
        this.row = row;
        this.column = column;
    }

    function Move(oldRow, oldColumn, row, column) {
        this.oldRow = oldRow;
        this.oldColumn = oldColumn;
        this.row = row;
        this.column = column;
    }

    function getCursorPosition(e) {
        var x;
        var y;
        if (e.pageX != undefined && e.pageY != undefined) {
            x = e.pageX;
            y = e.pageY;
        }
        else {
            x = e.clientX + document.body.scrollLeft + document.documentElement.scrollLeft;
            y = e.clientY + document.body.scrollTop + document.documentElement.scrollTop;
        }
        x -= canvas.offsetLeft;
        y -= canvas.offsetTop;
        x = Math.min(x, boardStep * boardStep);
        y = Math.min(y, boardStep * boardStep);
        var cell = new Cell(Math.floor(y / boardStep), Math.floor(x / boardStep));
        return cell;
    }

    function getFigure(row, column) {
        var selected;
        $.each(blackFigures, function (index, fig) {
            if (fig.column == column && fig.row == row) {
                selected = fig;
                return;
            }
        });

        if (selected)
            return selected;

        $.each(whiteFigures, function (index, fig) {
            if (fig.column == column && fig.row == row) {
                selected = fig;
                return;
            }
        });

        if (selected)
            return selected;
    }

    function log(message) {
        if (console)
            console.log(message);
    }

    function executeFunctionByName(functionName, context /*, args */) {
        var args = Array.prototype.slice.call(arguments).splice(2);
        var namespaces = functionName.split(".");
        var func = namespaces.pop();
        for (var i = 0; i < namespaces.length; i++) {
            context = context[namespaces[i]];
        }
        return context[func].apply(this, args);
    }

    public.getCurrentMoveNumber = function () {
        return moveNumber;
    };

    public.getMoves = function () {
        return moves;
    };

    public.getWhiteFigures = function () {
        return whiteFigures;
    };

    public.getBlackFigures = function () {
        return blackFigures;
    };

    return public;
};

var Notation = function () {
    var public = {};

    public.UndoMove = function (moveNum, color) {
        var row = $("#notation table tr[id='move_" + moveNum + "']");
        if (color === "black") {
            //$("td", row).eq(1).remove();
            var tds = $("td", row);
            tds.eq(1).removeClass("selected");
            tds.eq(0).addClass("selected");
        }
        else {
            $("td:first", row).removeClass("selected");
            $("td:last-child", row.prev()).addClass("selected");
            //row.remove();
        }
    };

    public.WriteMove = function (moveNumber, move) {
        var tokens = move.split(" ");
        var cell1 = $("<td></td>").html(moveNumber + ". " + tokens[0]);
        var cell2 = $("<td></td>").html(tokens[1]);
        var lastRow = $("#notation table tr:last");
        var row = $("<tr></tr>").append(cell1).append(cell2);
        lastRow.after(row);
    };

    public.PerformMove = function (move, figure, nextMoveIsWhite, moveNumber) {
        var parent = $("#notation table");

        var row = $("tr[id='move_" + moveNumber + "']", parent);
        if (row.length === 0) {
            row = $("<tr></tr>").attr("id", "move_" + moveNumber);
            parent.append(row);
        }

        var cells = $("td", row);

        if (nextMoveIsWhite) {
            var cell = cells.eq(1);
            if (cell.length === 0) {
                cell = $("<td></td>").html(moveToText(move, figure)).appendTo(row);
            }

            cell.addClass("selected");

            cell.prev().removeClass("selected");

        }
        else {
            var cell = cells.eq(0);
            if (cell.length === 0) {
                cell = $("<td></td>").html(moveNumber + ". " + moveToText(move, figure)).appendTo(row);
            }
            $("td:last-child", row.prev()).removeClass("selected");
            cell.addClass("selected");
        }
    };

    function moveToText(move, figure) {
        var oldText = getColorMove(figure, move.oldRow, move.oldColumn);
        var newText = getColorMove(figure, move.row, move.column);

        return oldText + " - " + newText;
    }

    function getColorMove(figure, row, column) {
        return getFigurePrefix(figure) + getRankName(column) + (8 - row);
    }

    function getFigurePrefix(figure) {

        switch (figure.type) {
            case "king":
                return "K";
            case "knight":
                return "N";
            case "queen":
                return "Q";
            case "bishop":
                return "B";
            case "pawn":
            default:
                return "";
        }
    }

    function getRankName(row) {
        return String.fromCharCode(row + 97);
    }

    return public;
};

String.prototype.format = function () {
    var args = arguments;
    return this.replace(/{(\d+)}/g, function (match, number) {
        return typeof args[number] != 'undefined'
      ? args[number]
      : match
    ;
    });
};