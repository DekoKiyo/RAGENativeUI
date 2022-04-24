using System;
using System.ComponentModel;
using System.Drawing;
using RAGENativeUI.Elements;

namespace RAGENativeUI.PauseMenu
{
    public class TabItem : IScrollableListItem
    {
        public TabItem(string name)
        {
            RockstarTile = new Sprite("pause_menu_sp_content", "rockstartilebmp", new Point(), new Size(64, 64), 0f, Color.FromArgb(40, 255, 255, 255));
            Title = name;
            DrawBg = true;
            UseDynamicPositionment = true;
        }

        public bool Visible { get; set; }
        public bool Focused { get; set; }
        public string Title { get; set; }
        
        [Obsolete("Use Selected instead", false)]
        public bool Active { get; set; }
        public bool JustOpened { get; set; }
        public bool CanBeFocused { get; set; }
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }
        public Point SafeSize { get; set; }
        public bool UseDynamicPositionment { get; set; }
        public TabView Parent { get; set; }

        public event EventHandler Activated;
        public event EventHandler DrawInstructionalButtons;
        public bool DrawBg;
        public bool FadeInWhenFocused { get; set; }

        public bool Selected { get; set; }

        public bool Skipped { get; set; }

        protected Sprite RockstarTile;

        public void OnActivated()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        public virtual void ProcessControls()
        {
        }

        public virtual void Draw()
        {
            if (!Visible) return;

            var res = UIMenu.GetScreenResolutionMantainRatio();

            if (UseDynamicPositionment)
            {
                SafeSize = new Point(300, 240);

                TopLeft = new Point(SafeSize.X, SafeSize.Y);
                BottomRight = new Point((int)res.Width - SafeSize.X, (int)res.Height - SafeSize.Y);
            }

            var rectSize = new Size(BottomRight.SubtractPoints(TopLeft));

            DrawInstructionalButtons?.Invoke(this, EventArgs.Empty);

            if (DrawBg)
            {
                ResRectangle.Draw(TopLeft, rectSize, Color.FromArgb((Focused || !FadeInWhenFocused) ? 200 : 120, 0, 0, 0));

                var tileSize = 100;
                RockstarTile.Size = new Size(tileSize, tileSize);

                var cols = rectSize.Width / tileSize;
                var fils = 4;

                for (int i = 0; i < cols * fils; i++)
                {
                    RockstarTile.Position = TopLeft.AddPoints(new Point(tileSize * (i % cols), tileSize * (i / cols)));
                    RockstarTile.Color = Color.FromArgb((int)MiscExtensions.LinearFloatLerp(40, 0, i / cols, fils), 255, 255, 255);
                    RockstarTile.Draw();
                }
            }
        }

        public virtual void DrawTextures(Rage.Graphics g)
        { }
    }
}

