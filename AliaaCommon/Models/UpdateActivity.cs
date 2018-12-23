using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon.Models
{
    class UpdateActivity : UserActivity
    {
        public List<Variance> Diff { get; set; }

        public UpdateActivity()
        {
            ActivityType = ActivityType.Update;
        }

        public void SetDiff<T>(T oldObj, T newObj)
        {
            Diff = new List<Variance>();
            foreach (var p in typeof(T).GetProperties())
            {
                object oldVal = p.GetValue(oldObj);
                object newVal = p.GetValue(newObj);
                bool areEqual = newVal.Equals(oldVal);
                if (!areEqual && !(newVal is string) && (newVal is IEnumerable))
                {
                    areEqual = true;
                    var enOld = ((IEnumerable)oldVal).GetEnumerator();
                    var enNew = ((IEnumerable)newVal).GetEnumerator();
                    while (true)
                    {
                        bool oldHasNext = enOld.MoveNext();
                        bool newHasNext = enNew.MoveNext();
                        if (!oldHasNext && !newHasNext)
                            break;
                        if (oldHasNext != newHasNext)
                        {
                            areEqual = false;
                            break;
                        }
                        if(oldHasNext && !enOld.Current.Equals(enNew.Current))
                        {
                            areEqual = false;
                            break;
                        }
                    }
                }
                if (!areEqual)
                    Diff.Add(new Variance { Prop = p.Name, OldValue = oldVal, NewValue = newVal });
            }
        }

        public class Variance
        {
            public string Prop { get; set; }
            public object OldValue { get; set; }
            public object NewValue { get; set; }
        }
    }
}
