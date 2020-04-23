using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YDSCodeSample.Models
{
    public class Model
    {
        public int Id;

        public override bool Equals(object obj)
        {
            if (obj is Model)
                if (Id == ((Model)obj).Id)
                    return true;

            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
