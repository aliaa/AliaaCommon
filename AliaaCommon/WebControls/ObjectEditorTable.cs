using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AliaaCommon.WebControls
{
    public class ObjectEditorTable
    {
        public enum ControlType
        {
            Text,
            Number,
            Combo,
            Check,
            Unknown,
        }

        public static void CreateUI(Table table, Type type, bool enabled = true, int columnCount = 2, Dictionary<string, int> fieldsColSpan = null,
            Dictionary<string, ControlType> overrideTypes = null, Dictionary<string, List<ListItem>> comboItems = null, string[] excludeFields = null)
        {
            table.Rows.Clear();
            TableRow row = null;
            int i = 0;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (excludeFields != null && excludeFields.Contains(prop.Name))
                    continue;
                Type ptype = prop.PropertyType;
                ControlType controlType;
                if (overrideTypes != null && overrideTypes.ContainsKey(prop.Name))
                    controlType = overrideTypes[prop.Name];
                else
                    controlType = GetControlType(ptype);
                if (controlType == ControlType.Unknown)
                    continue;

                if (i % columnCount == 0)
                {
                    if (i != 0)
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

                RequiredAttribute reqAttr = prop.GetCustomAttribute<RequiredAttribute>();
                if (reqAttr != null)
                {
                    RequiredFieldValidator reqValidator = new RequiredFieldValidator
                    {
                        ControlToValidate = controlID,
                        ForeColor = Color.Red,
                        Text = "*",
                        ErrorMessage = reqAttr.ErrorMessage
                    };
                    validationCell.Controls.Add(reqValidator);
                }
                RegularExpressionAttribute regexAttr = prop.GetCustomAttribute<RegularExpressionAttribute>();
                if (regexAttr != null)
                {
                    RegularExpressionValidator regexValidator = new RegularExpressionValidator
                    {
                        ControlToValidate = controlID,
                        ForeColor = Color.Red,
                        Text = "*",
                        ValidationExpression = regexAttr.Pattern,
                        ErrorMessage = regexAttr.ErrorMessage
                    };
                    validationCell.Controls.Add(regexValidator);
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
                        if (comboItems != null && comboItems.ContainsKey(prop.Name))
                        {
                            foreach (ListItem item in comboItems[prop.Name])
                                (ctrl as DropDownList).Items.Add(item);
                        }
                        else
                        {
                            Array enumVals = ptype.GetEnumValues();
                            for (int j = 0; j < enumVals.Length; j++)
                            {
                                string name = enumVals.GetValue(j).ToString();
                                string dispName = Utils.GetDisplayNameOfMember(ptype, name);
                                (ctrl as DropDownList).Items.Add(new ListItem(dispName, name));
                            }
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
                if (fieldsColSpan != null && fieldsColSpan.ContainsKey(prop.Name))
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

        public static void FillFromObject<T>(Table table, T obj, Dictionary<string, ControlType> overrideTypes = null)
        {
            if (obj == null)
                return;
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                Control ctrl = GetControl(table, prop.Name);
                if (ctrl == null)
                    continue;
                ControlType controlType;
                if (overrideTypes != null && overrideTypes.ContainsKey(prop.Name))
                    controlType = overrideTypes[prop.Name];
                else
                    controlType = GetControlType(prop.PropertyType);
                if (controlType == ControlType.Unknown)
                    continue;
                object value = prop.GetValue(obj);
                if (value == null)
                    continue;
                switch (controlType)
                {
                    case ControlType.Text:
                    case ControlType.Number:
                        (ctrl as TextBox).Text = value.ToString();
                        break;
                    case ControlType.Combo:
                        (ctrl as DropDownList).SelectedValue = value.ToString();
                        break;
                    case ControlType.Check:
                        (ctrl as CheckBox).Checked = (bool)value;
                        break;
                    default:
                        break;
                }
            }
        }

        public static Control GetControl(Table table, string propName)
        {
            string controlID = "ac_" + propName;
            return table.FindControl(controlID);
        }

        public static void FillToObject<T>(Table table, T obj, Dictionary<string, ControlType> overrideTypes = null)
        {
            if (obj == null)
                return;
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                Type ptype = prop.PropertyType;
                Control ctrl = GetControl(table, prop.Name);
                if (ctrl == null)
                    continue;
                ControlType controlType;
                if (overrideTypes != null && overrideTypes.ContainsKey(prop.Name))
                    controlType = overrideTypes[prop.Name];
                else
                    controlType = GetControlType(ptype);
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
                        else if (ptype == typeof(long))
                            prop.SetValue(obj, long.Parse(valueStr));
                        else if (ptype == typeof(short))
                            prop.SetValue(obj, short.Parse(valueStr));
                        else if (ptype == typeof(byte))
                            prop.SetValue(obj, byte.Parse(valueStr));
                        break;

                    case ControlType.Combo:
                        string selectedStr = (ctrl as DropDownList).SelectedValue;
                        try
                        {
                            prop.SetValue(obj, Enum.Parse(ptype, selectedStr));
                        }
                        catch { }
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
