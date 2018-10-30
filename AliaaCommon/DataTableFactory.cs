using AliaaCommon;
using AliaaCommon.MongoDB;
using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace AliaaCommon
{
    public class DataTableFactory
    {
        protected readonly Type thisType;
        protected readonly MongoHelper DB;

        public DataTableFactory(MongoHelper DB)
        {
            thisType = typeof(DataTableFactory);
            this.DB = DB;
        }

        protected DataTableFactory(MongoHelper DB, Type thisType)
        {
            this.thisType = thisType;
            this.DB = DB;
        }
        
        private Dictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>();

        public DataTable Create<T>(bool convertDateToPersian = true, bool includeTimeInDates = true, bool addIndexColumn = false, string[] excludeColumns = null) where T : MongoEntity
        {
            return Create(DB.All<T>(), convertDateToPersian, includeTimeInDates, addIndexColumn, excludeColumns);
        }

        public DataTable Create<T>(IEnumerable<T> data, bool convertDateToPersian = true, bool includeTimeInDates = true, bool addIndexColumn = false, string[] excludeColumns = null)
        {
            Type type = typeof(T);
            MethodInfo method;
            if (methods.ContainsKey(type))
                method = methods[type];
            else
            {
                method = thisType.GetMethod("For" + type.Name);
                methods.Add(type, method);
            }
            if (method == null)
                return CreateDataTable(new DataTable(), data, convertDateToPersian, includeTimeInDates, addIndexColumn, excludeColumns);
            return (DataTable)method.Invoke(this, new object[] { data, convertDateToPersian, includeTimeInDates, addIndexColumn, excludeColumns });
        }

        protected static string INDEX_COLUMN = "ردیف";

        public Dictionary<PropertyInfo, string> CreateDataTableColumns<T>(DataTable table, bool convertDateToPersian = true, bool includeTimeInDates = true, bool addIndexColumn = false, string[] excludeColumns = null)
        {
            PropertyInfo[] props = typeof(T).GetProperties();
            Dictionary<PropertyInfo, string> displayNames = new Dictionary<PropertyInfo, string>();
            if (addIndexColumn)
                table.Columns.Add(INDEX_COLUMN, typeof(int));

            foreach (PropertyInfo p in props)
            {
                if (excludeColumns != null && excludeColumns.Contains(p.Name))
                    continue;
                string dispName = Utils.GetDisplayNameOfMember(p);
                displayNames.Add(p, dispName);
                if (table.Columns.Contains(dispName))
                    continue;

                Type propType = p.PropertyType;
                if (propType.IsEquivalentTo(typeof(ObjectId)) || propType.IsEnum || p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                    propType = typeof(string);
                else if (propType == typeof(DateTime) && (!includeTimeInDates || convertDateToPersian))
                    propType = typeof(string);
                else
                {
                    Type undelying = Nullable.GetUnderlyingType(propType);
                    if (undelying != null)
                        propType = undelying;
                }
                DataColumn col = new DataColumn(dispName, propType);
                table.Columns.Add(col);
            }
            return displayNames;
        }

        public DataTable CreateDataTable<T>(DataTable table, IEnumerable<T> list, bool convertDateToPersian = true, bool includeTimeInDates = true, bool addIndexColumn = false, string[] excludeColumns = null)
        {
            if (list == null)
                return null;
            Dictionary<PropertyInfo, string> displayNames = CreateDataTableColumns<T>(table, convertDateToPersian, includeTimeInDates, addIndexColumn, excludeColumns);

            int i = 1;
            foreach (T item in list)
            {
                DataRow row = table.NewRow();
                if (addIndexColumn)
                    row[INDEX_COLUMN] = i++;
                foreach (PropertyInfo p in displayNames.Keys)
                {
                    object value = p.GetValue(item);
                    if (value is ObjectId)
                    {
                        value = value.ToString();
                    }
                    else if (p.PropertyType.IsEnum)
                        value = Utils.GetDisplayNameOfMember(p.PropertyType, value.ToString());
                    else if (value is DateTime && convertDateToPersian)
                        value = Utils.GetPersianDateString((DateTime)value, includeTimeInDates);
                    else if (value is IEnumerable && !(value is string))
                    {
                        StringBuilder sb = new StringBuilder();
                        Type itemsType = null;
                        foreach (var v in (IEnumerable)value)
                        {
                            if (itemsType == null)
                                itemsType = v.GetType();
                            sb.Append(Utils.GetDisplayNameOfMember(itemsType, v.ToString())).Append(" ; ");
                        }
                        if (sb.Length > 3)
                            sb.Remove(sb.Length - 3, 3);
                        value = sb.ToString();
                    }
                    row[displayNames[p]] = value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

    }
}