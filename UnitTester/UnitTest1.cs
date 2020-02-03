using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VNC.dbvn;

namespace UnitTester
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestDBColumn()
        {
            DBColumn dbc = new DBColumn("admin(NOT_NULL,)");
            Assert.AreEqual(false, dbc.Nullable);
            Assert.AreEqual(ColumnDateType.None, dbc.DateType);
            Assert.AreEqual("admin", dbc.Name);
            dbc.ID = 10;
            Assert.AreEqual(10, dbc.ID);
            dbc.ID = 2;
            Assert.AreEqual(10, dbc.ID);

            dbc = new DBColumn("admin(NULL,datetime)");
            Assert.AreEqual(true, dbc.Nullable);
            Assert.AreEqual(ColumnDateType.DateTime, dbc.DateType);
            dbc = new DBColumn("admin(NULL,date)");
            Assert.AreEqual(true, dbc.Nullable);
            Assert.AreEqual(ColumnDateType.Date, dbc.DateType);

            dbc = new DBColumn("admin", false, ColumnDateType.None);
            Assert.AreEqual(false, dbc.Nullable);
            Assert.AreEqual(ColumnDateType.None, dbc.DateType);
            Assert.AreEqual("admin", dbc.Name);
            dbc = new DBColumn("admin", true, ColumnDateType.DateTime);
            Assert.AreEqual(true, dbc.Nullable);
            Assert.AreEqual(ColumnDateType.DateTime, dbc.DateType);
            Assert.AreEqual("admin", dbc.Name);
        }
    }
}
