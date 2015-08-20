using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;

namespace EduChess
{
    public class HomeController : UIViewController
    {
        public override void ViewWillAppear(bool animated)
        {
            View.BackgroundColor = UIColor.White;

            var board = new BoardView();
            Add(board);

            var control = new ControlView(board);
            Add(control);

            board.SetControl(control);

            View.ConstrainLayout(() =>
                                 board.Frame.Width == BoardView.BoardWidth && 
                                 board.Frame.Height == BoardView.BoardWidth && 
                                 board.Frame.Left == View.Frame.Left + 26 &&
                                 board.Frame.Top == View.Frame.Top + 26 &&
                                 control.Frame.Right == View.Frame.Right - 26 &&
                                 control.Frame.Height == board.Frame.Height &&
                                 control.Frame.Left == board.Frame.Right + 26 &&
                                 control.Frame.Top == board.Frame.Top
                                 );
        }
    }
}




