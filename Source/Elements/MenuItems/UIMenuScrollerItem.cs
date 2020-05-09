﻿namespace RAGENativeUI.Elements
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Implements the basic functionality of an item with multiple options to choose from through scrolling, with left/right arrows.
    /// </summary>
    public abstract class UIMenuScrollerItem : UIMenuItem
    {
        /// <summary>
        /// Defines the value of <see cref="Index"/> when <see cref="IsEmpty"/> is <c>true</c>.
        /// </summary>
        public const int EmptyIndex = -1;

        private int index = EmptyIndex;

        /// <summary>
        /// Gets or sets the index of the selected option. When <see cref="IsEmpty"/> is <c>true</c>, <see cref="EmptyIndex"/> is returned.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <c>value</c> is not <see cref="EmptyIndex"/> when <see cref="IsEmpty"/> is <c>true</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <c>value</c> is negative.
        /// -or-
        /// <c>value</c> is equal to or greater than <see cref="OptionCount"/>.
        /// </exception>
        public virtual int Index 
        {
            get
            {
                if (IsEmpty)
                {
                    return EmptyIndex;
                }

                int idx = index;
                int count = OptionCount;
                if (idx < 0 || idx >= count)
                {
                    // in case OptionCount changed and index is now out of bounds or index has EmptyIndex and the scroller is no longer empty
                    int oldIndex = idx;
                    idx = idx == EmptyIndex ? 0 : (idx % count);
                    index = idx;
                    OnSelectedIndexChanged(oldIndex, idx);
                }

                return idx;
            }
            set
            {
                int oldIndex = Index;
                if (value != oldIndex)
                {
                    if (IsEmpty && value != EmptyIndex)
                    {
                        throw new ArgumentException(nameof(value), $"{nameof(IsEmpty)} is true and {nameof(value)} is not equal to {nameof(EmptyIndex)}");
                    }

                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} is negative");
                    }

                    if (value >= OptionCount)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} is equal or greater than {nameof(OptionCount)}");
                    }

                    index = value;
                    OnSelectedIndexChanged(oldIndex, value);
                }
            }
        }

        /// <summary>
        /// Gets the number of possible options.
        /// </summary>
        public abstract int OptionCount { get; }

        /// <summary>
        /// Gets the text to display as the currently selected option.
        /// </summary>
        /// <remarks>
        /// This property is also used when <see cref="IsEmpty"/> is <c>true</c>, so the implementation needs to take into account this
        /// state, for example it may return <c>null</c>.
        /// </remarks>
        public abstract string OptionText { get; }

        /// <summary>
        /// Gets whether any option is available.
        /// </summary>
        /// <returns>
        /// <c>true</c> when <see cref="OptionCount"/> is zero or negative; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEmpty => OptionCount <= 0;

        /// <summary>
        /// Occurs when the value of <see cref="Index"/> changes, either programmatically or when the user interacts with the item.
        /// </summary>
        public event ItemScrollerEvent IndexChanged;

        /// <summary>
        /// Gets or sets whether scrolling through the options is enabled.
        /// </summary>
        public bool ScrollingEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether scrolling through the options is enabled when <see cref="UIMenuItem.Enabled"/> is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// The property <see cref="ScrollingEnabled"/> still has to be <c>true</c> to enable scrolling when the item is disabled.
        /// </remarks>
        public bool ScrollingEnabledWhenDisabled { get; set; } = false;

        /// <inheritdoc/>
        public override string RightLabel { get => base.RightLabel; set => throw new Exception($"{nameof(UIMenuScrollerItem)} cannot have a right label."); }

        // temp until menu controls are refactored
        internal uint HoldTime;
        internal uint HoldTimeBeforeScroll = 200;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIMenuScrollerItem"/> class.
        /// </summary>
        /// <param name="text">The <see cref="UIMenuScrollerItem"/>'s label.</param>
        /// <param name="description">The <see cref="UIMenuScrollerItem"/>'s description.</param>
        public UIMenuScrollerItem(string text, string description) : base(text, description)
        {
            ScrollerProxy = new UIMenuScrollerProxy(this);
        }

        /// <summary>
        /// Scrolls to the next option following the selected option.
        /// </summary>
        public virtual void ScrollToNextOption(UIMenu menu = null)
        {
            if (IsEmpty)
            {
                return;
            }

            int oldIndex = Index;
            int newIndex = oldIndex + 1;

            if (newIndex > (OptionCount - 1)) // wrap around
                newIndex = 0;

            Index = newIndex;

            if (menu != null && oldIndex != newIndex)
            {
                menu.ScrollerChange(this, oldIndex, newIndex);
            }
        }

        /// <summary>
        /// Scrolls to the option previous to the selected option.
        /// </summary>
        public virtual void ScrollToPreviousOption(UIMenu menu = null)
        {
            if (IsEmpty)
            {
                return;
            }

            int oldIndex = Index;
            int newIndex = oldIndex - 1;

            if (newIndex < 0) // wrap around
                newIndex = OptionCount - 1;

            Index = newIndex;

            if (menu != null && oldIndex != newIndex)
            {
                menu.ScrollerChange(this, oldIndex, newIndex);
            }
        }

        /// <inheritdoc/>
        public override void Draw(float x, float y, float width, float height)
        {
            base.Draw(x, y, width, height);

            string selectedOption = OptionText ?? string.Empty;

            SetTextCommandOptions(false);
            float optTextWidth = TextCommands.GetWidth(selectedOption);

            GetBadgeOffsets(out _, out float badgeOffset);

            if (Selected && (Enabled || ScrollingEnabledWhenDisabled) && ScrollingEnabled)
            {
                Color arrowsColor = CurrentForeColor;
                if (!Enabled)
                {
                    arrowsColor = HighlightedForeColor;
                }

                float optTextX = x + width - (0.00390625f * 1.5f) - optTextWidth - (0.0046875f * 1.5f) - badgeOffset;
                float optTextY = y + 0.00277776f;

                SetTextCommandOptions(false);
                TextCommands.Display(selectedOption, optTextX, optTextY);

                {
                    UIMenu.GetTextureDrawSize(UIMenu.CommonTxd, UIMenu.ArrowRightTextureName, out float w, out float h);

                    float spriteX = x + width - (0.00390625f * 0.5f) - (w * 0.5f) - badgeOffset;
                    float spriteY = y + (0.034722f * 0.5f);

                    UIMenu.DrawSprite(UIMenu.CommonTxd, UIMenu.ArrowRightTextureName, spriteX, spriteY, w, h, arrowsColor);
                }
                {
                    UIMenu.GetTextureDrawSize(UIMenu.CommonTxd, UIMenu.ArrowLeftTextureName, out float w, out float h);

                    float spriteX = x + width - (0.00390625f * 1.5f) - (w * 0.5f) - optTextWidth - (0.0046875f * 1.5f) - badgeOffset;
                    float spriteY = y + (0.034722f * 0.5f);

                    UIMenu.DrawSprite(UIMenu.CommonTxd, UIMenu.ArrowLeftTextureName, spriteX, spriteY, w, h, arrowsColor);
                }
            }
            else
            {
                float optTextX = x + width - 0.00390625f - optTextWidth - badgeOffset;
                float optTextY = y + 0.00277776f;

                SetTextCommandOptions(false);
                if (!ScrollingEnabled)
                {
                    Internals.CTextStyle.ScriptStyle.Color = DisabledForeColor.ToArgb();
                }
                TextCommands.Display(selectedOption, optTextX, optTextY);
            }
        }

        protected internal override bool OnInput(UIMenu menu, Common.MenuControls control)
        {
            bool consumed = base.OnInput(menu, control);

            if (ScrollingEnabled && (Enabled || ScrollingEnabledWhenDisabled))
            {
                switch (control)
                {
                    case Common.MenuControls.Left:
                        consumed = true;
                        Common.PlaySound(menu.AUDIO_LEFTRIGHT, menu.AUDIO_LIBRARY);
                        ScrollToPreviousOption();
                        break;

                    case Common.MenuControls.Right:
                        consumed = true;
                        Common.PlaySound(menu.AUDIO_LEFTRIGHT, menu.AUDIO_LIBRARY);
                        ScrollToNextOption();
                        break;
                }
            }

            return consumed;
        }

        protected internal override bool OnMouseInput(UIMenu menu, RectangleF itemBounds, PointF mousePos, MouseInput input)
        {
            if (menu == null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            bool consumed = false;
            if (Selected && Hovered)
            {
                bool inSelectBounds = false;
                float selectBoundsX = itemBounds.X + itemBounds.Width * 0.33333f;

                if (mousePos.X <= selectBoundsX)
                {
                    inSelectBounds = true;
                    // approximately hovering the label, first 1/3 of the item width
                    // TODO: game shows cursor sprite 5 when hovering this part, but only if the item does something when selected.
                    //       Here, we don't really know if the user does something when selected, maybe add some bool property in UIMenuListItem?
                    if (input == MouseInput.JustReleased)
                    {
                        consumed = true;
                        OnInput(menu, Common.MenuControls.Select);
                    }
                }

                if (!inSelectBounds && ScrollingEnabled && (Enabled || ScrollingEnabledWhenDisabled) && input == MouseInput.Pressed)
                {
                    UIMenu.GetTextureDrawSize(UIMenu.CommonTxd, UIMenu.ArrowRightTextureName, out float rightW, out _);

                    float rightX = (0.00390625f * 1.0f) + (rightW * 1.0f) + (0.0046875f * 0.75f);

                    if (menu.ScaleWithSafezone)
                    {
                        N.SetScriptGfxAlign('L', 'T');
                        N.SetScriptGfxAlignParams(-0.05f, -0.05f, 0.0f, 0.0f);
                    }
                    N.GetScriptGfxPosition(rightX, 0.0f, out rightX, out _);
                    N.GetScriptGfxPosition(0.0f, 0.0f, out float borderX, out _);
                    if (menu.ScaleWithSafezone)
                    {
                        N.ResetScriptGfxAlign();
                    }

                    rightX = itemBounds.Right - rightX + borderX;

                    // It does not check if the mouse in exactly on top of the arrow sprites intentionally:
                    //  - If to the right of the right arrow's left border, go right
                    //  - Anywhere else in the item, go left.
                    // This is how the vanilla menus behave
                    consumed = true;
                    if (mousePos.X >= rightX)
                    {
                        OnInput(menu, Common.MenuControls.Right);
                    }
                    else
                    {
                        OnInput(menu, Common.MenuControls.Left);
                    }
                }
            }

            return consumed;
        }

        /// <summary>
        /// Triggers <see cref="IndexChanged"/> event.
        /// </summary>
        protected virtual void OnSelectedIndexChanged(int oldIndex, int newIndex)
        {
            IndexChanged?.Invoke(this, oldIndex, newIndex);
        }
    }

    /// <summary>
    /// Helper class to allow to share code between <see cref="UIMenuScrollerItem"/> and <see cref="UIMenuListItem"/>.
    /// </summary>
    internal sealed class UIMenuScrollerProxy
    {
        public delegate ref uint GetHoldTimeDelegate();

        public UIMenuItem Item { get; }
        public Func<bool> GetScrollingEnabled { get; }
        public Func<bool> GetScrollingEnabledWhenDisabled { get; }
        public Func<uint> GetHoldTimeBeforeScroll { get; }
        public GetHoldTimeDelegate GetHoldTime { get; }
        public Action<int> SetIndex { get; }

        public UIMenuScrollerProxy(UIMenuScrollerItem item)
        {
            Item = item;
            GetScrollingEnabled = () => item.ScrollingEnabled;
            GetScrollingEnabledWhenDisabled = () => item.ScrollingEnabledWhenDisabled;
            GetHoldTimeBeforeScroll = () => item.HoldTimeBeforeScroll;
            GetHoldTime = () => ref item.HoldTime;
            SetIndex = i => item.Index = i;
        }

        public UIMenuScrollerProxy(UIMenuListItem item)
        {
            Item = item;
            GetScrollingEnabled = () => item.ScrollingEnabled;
            GetScrollingEnabledWhenDisabled = () => false;
            GetHoldTimeBeforeScroll = () => item.HoldTimeBeforeScroll;
            GetHoldTime = () => ref item._holdTime;
            SetIndex = i => item.Index = i;
        }
    }
}
