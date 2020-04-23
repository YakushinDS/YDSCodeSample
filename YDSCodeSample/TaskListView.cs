using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YDSCodeSample
{
    public class TaskListView : ListView
    {
        private bool checkFromDoubleClick = false;
        private bool checkFromSpacebar = false;

        protected override void OnItemCheck(ItemCheckEventArgs ice)
        {
            if (checkFromDoubleClick)
            {
                ice.NewValue = ice.CurrentValue;
                checkFromDoubleClick = false;
            }
            else if (checkFromSpacebar)
            {
                ice.NewValue = ice.CurrentValue;
            }
            else
                base.OnItemCheck(ice);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (e.Clicks > 1))
            {
                checkFromDoubleClick = true;
            }
            base.OnMouseDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            checkFromDoubleClick = false;
            if (e.KeyCode == Keys.Space)
                checkFromSpacebar = true;
            base.OnKeyDown(e);
        }
    }
}
