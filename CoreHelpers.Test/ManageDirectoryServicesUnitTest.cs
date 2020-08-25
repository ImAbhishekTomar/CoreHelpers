using CoreHelpers.DirectoryServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreHelpers.Test
{
    [TestClass]
    public class ManageDirectoryServicesUnitTest
    {
        const string MSID = "XXXXXX";
        const string MSPASSWORD = "XXXXXXXXXX";
        const string MANAGER_NTID = "XXXXXX";
        const string EMAIL = "email@domain.com";
        const string EMPLOYEEID = "000000000";
        ManageDirectoryServices manageDirectoryServices = null;

        [TestMethod]
        public void ContractorInitialsSuccess()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices(MSID);
            var context = manageDirectoryServices.User;
            Assert.AreEqual(MSID, context.SamAccountName);
        }

        [TestMethod]
        public void SignInSuccess()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var context = manageDirectoryServices.SignIn(MSID, MSPASSWORD);
            Assert.AreEqual(EMAIL, context.Context.User.EmailAddress);
        }

        [TestMethod]
        public void GetEmployeeByNTIDSuccess()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var context = manageDirectoryServices.GetEmployeeByNTID(MSID);
            Assert.AreEqual(MSID, context.User.SamAccountName);
        }

        [TestMethod]
        public void GetManagerSuccess()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var context = manageDirectoryServices.GetEmployeeByNTID(MSID);
            var manager = context.GetManager();
            Assert.AreEqual(MANAGER_NTID, context.Manager.SamAccountName);
        }

        [TestMethod]
        public void GetReportesSuccess()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var context = manageDirectoryServices.GetEmployeeByNTID(MSID);
            var repcontext = context.GetDirectReports();
            var flag = repcontext.DirectReports.Count > 0 ? true : false;
            Assert.AreEqual(true, flag);
        }

        [TestMethod]
        public void GetEmployeeByEmailSuccess()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var context = manageDirectoryServices.GetEmployeeByEmail(EMAIL);
            Assert.AreEqual(EMAIL, context.User.EmailAddress);
        }

        [TestMethod]
        public void GetEmployeeByEmployeeIdSuccess()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var context = manageDirectoryServices.GetEmployeeByEmpId(EMPLOYEEID);
            Assert.AreEqual(EMPLOYEEID, context.User.EmployeeId);
        }


        [TestMethod]
        public void IsUserExistSuccess()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var IsExist = manageDirectoryServices.IsUserExist(MSID);
            Assert.AreEqual(true, IsExist);
        }

        [TestMethod]
        public void IsUserExistFail()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var IsExist = manageDirectoryServices.IsUserExist("invalidid");
            Assert.AreEqual(false, IsExist);
        }


        [TestMethod]
        public void GetEmployeeSuccess()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var searchTemplate = new System.DirectoryServices.AccountManagement.UserPrincipal(manageDirectoryServices.Context)
            {
                SamAccountName = MSID,
                EmailAddress = EMAIL,
                EmployeeId = EMPLOYEEID
            };
            var context = manageDirectoryServices.GetEmployee(searchTemplate);
            Assert.AreEqual(MSID, context.User.SamAccountName);
        }


        [TestMethod]
        public void GetEmployeeNameSuccess0()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var repcontext = manageDirectoryServices.GetEmployee("dana", "givenname");
            var flag = repcontext.DirectReports.Count > 0 ? true : false;
            Assert.AreEqual(true, flag);
        }


        [TestMethod]
        public void GetEmployeeNameSuccess1()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var repcontext = manageDirectoryServices.GetEmployee("dana", "name");
            var flag = repcontext.DirectReports.Count > 0 ? true : false;
            Assert.AreEqual(true, flag);
        }


        [TestMethod]
        public void GetEmployeeNameSuccess2()
        {
            ManageDirectoryServices manageDirectoryServices = new ManageDirectoryServices();
            var repcontext = manageDirectoryServices.GetEmployee("what, dana", "displayname");
            var flag = repcontext.DirectReports.Count > 0 ? true : false;
            Assert.AreEqual(true, flag);
        }
    }
}
