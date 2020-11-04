using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalDelivery
{
    public static class APIConstant
    {
        #region Databases
        public const string LogisticsDatabase = "test";

        #endregion

        #region Collections

        public const string CitiesCollection = "cities";
        public const string PlanesCollection = "planes";
        public const string CargoCollection = "cargo";

        #endregion

        #region Cargo Status

        public const string InProcess = "in process";
        public const string Delivered = "delivered";

        #endregion
        

    }
}
