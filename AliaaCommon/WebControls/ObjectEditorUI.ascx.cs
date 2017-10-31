using AliaaCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AliaaCommon.WebControls
{
    public partial class ObjectEditorUI : System.Web.UI.UserControl
    {
        private enum ControlType
        {
            Text,
            Number,
            Combo,
            Check,
            Unknown,
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void CreateUI(Type type, bool enabled = true, int columnCount = 2, Dictionary<string, int> fieldsColSpan = null)
        {
            if(table == null)
            {
                table = new Table();
                Controls.Add(table);
            }
            TableRow row = null;
            int i = 0;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                Type ptype = prop.PropertyType;
                ControlType controlType = GetControlType(ptype);
                if (controlType == ControlType.Unknown)
                    continue;

                if (i % columnCount == 0)
                {
                    if(i != 0)
                        table.Rows.Add(row);
                    row = new TableRow();
                }
                
                if (fieldsColSpan != null && fieldsColSpan.ContainsKey(prop.Name) && i % columnCount + fieldsColSpan[prop.Name] > columnCount)
                {
                    table.Rows.Add(row);
                    row = new TableRow();
                    i += columnCount - (i % columnCount);
                }
                
                Label lbl = new Label { Text = Utils.GetDisplayNameOfMember(prop) + ":" };
                TableCell titleCell = new TableCell();
                titleCell.Controls.Add(lbl);
                row.Cells.Add(titleCell);

                TableCell validationCell = new TableCell();
                string controlID = "ac_" + prop.Name;
                if(prop.GetCustomAttribute<RequiredAttribute>() != null)
                {
                    RequiredFieldValidator reqValidator = new RequiredFieldValidator { ControlToValidate = controlID, ForeColor = Color.Red, Text = "*" };
                    validationCell.Controls.Add(reqValidator);
                }
                row.Cells.Add(validationCell);

                WebControl ctrl = null;
                switch (controlType)
                {
                    case ControlType.Text:
                        ctrl = new TextBox();
                        break;
                    case ControlType.Number:
                        ctrl = new TextBox { TextMode = TextBoxMode.Number };
                        break;
                    case ControlType.Combo:
                        ctrl = new DropDownList();
                        Array enumVals = ptype.GetEnumValues();
                        for (int j = 0; j < enumVals.Length; j++)
                        {
                            string name = enumVals.GetValue(j).ToString();
                            string dispName = Utils.GetDisplayNameOfMember(ptype, name);
                            (ctrl as DropDownList).Items.Add(new ListItem(dispName, name));
                        }
                        break;
                    case ControlType.Check:
                        ctrl = new CheckBox();
                        break;
                    default:
                        break;
                }
                if (ctrl == null)
                    continue;
                ctrl.Enabled = enabled;
                ctrl.ID = controlID;
                TableCell ctrlCell = new TableCell();
                if(fieldsColSpan != null && fieldsColSpan.ContainsKey(prop.Name))
                {
                    int span = fieldsColSpan[prop.Name];
                    ctrlCell.ColumnSpan = (span - 1) * 3 + 1;
                    i += span - 1;
                    ctrl.Style.Add("width", "99%");
                }
                ctrlCell.Controls.Add(ctrl);
                row.Cells.Add(ctrlCell);

                i++;
            }
            table.Rows.Add(row);
        }

        private static ControlType GetControlType(Type ptype)
        {
            if (ptype == typeof(int) || ptype == typeof(long) || ptype == typeof(short) || ptype == typeof(byte))
                return ControlType.Number;
            if (ptype == typeof(string))
                return ControlType.Text;
            if (ptype == typeof(bool))
                return ControlType.Check;
            if (ptype.IsEnum)
                return ControlType.Combo;
            return ControlType.Unknown;
        }

        public void FillFromObject<T>(T obj)
        {
            if (obj == null)
                return;
            Type type = typeof(T);
            foreach (PropertyInfo prop in type.GetProperties())
            {
                Type ptype = prop.PropertyType;
                string controlID = "ac_" + prop.Name;
                Control ctrl = table.FindControl(controlID);
                if (ctrl == null)
                    continue;
                ControlType controlType = GetControlType(ptype);
                if (controlType == ControlType.Unknown)
                    continue;
                switch (controlType)
                {
                    case ControlType.Text:
                    case ControlType.Number:
                        (ctrl as TextBox).Text = prop.GetValue(obj).ToString();
                        break;
                    case ControlType.Combo:
                        (ctrl as DropDownList).SelectedValue = prop.GetValue(obj).ToString();
                        break;
                    case ControlType.Check:
                        (ctrl as CheckBox).Checked = (bool)prop.GetValue(obj);
                        break;
                    default:
                        break;
                }
            }
        }

        public void FillToObject<T>(T obj)
        {
            if (obj == null)
                return;
            Type type = typeof(T);
            foreach (PropertyInfo prop in type.GetProperties())
            {
                Type ptype = prop.PropertyType;
                string controlID = "ac_" + prop.Name;
                Control ctrl = table.FindControl(controlID);
                if (ctrl == null)
                    continue;
                ControlType controlType = GetControlType(ptype);
                if (controlType == ControlType.Unknown)
                    continue;
                switch (controlType)
                {
                    case ControlType.Text:
                        prop.SetValue(obj, (ctrl as TextBox).Text);
                        break;

                    case ControlType.Number:
                        string valueStr = (ctrl as TextBox).Text;
                        if (string.IsNullOrEmpty(valueStr))
                            break;
                        if (ptype == typeof(int))
                            prop.SetValue(obj, int.Parse(valueStr));
                        else if(ptype == typeof(long))
                            prop.SetValue(obj, long.Parse(valueStr));
                        else if (ptype == typeof(short))
                            prop.SetValue(obj, short.Parse(valueStr));
                        else if (ptype == typeof(byte))
                            prop.SetValue(obj, byte.Parse(valueStr));
                        break;

                    case ControlType.Combo:
                        string selectedStr = (ctrl as DropDownList).SelectedValue;
                        prop.SetValue(obj, Enum.Parse(ptype, selectedStr));
                        break;

                    case ControlType.Check:
                        prop.SetValue(obj, (ctrl as CheckBox).Checked);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}