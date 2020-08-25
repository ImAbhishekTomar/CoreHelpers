using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CoreHelpers.DirectoryServices
{
    #region RefranceHelpers
    /*
        //https://www.c-sharpcorner.com/article/active-directory-and-net/
        //https://ianatkinson.net/computing/adcsharp.htm
        //https://ianatkinson.net/computing/adcsharp.htm
    */
    #endregion

    public enum SearchBy
    {
        StartNTID = 0,
    }
    public class ManageDirectoryServices : IDisposable
    {
        public ContextType contextType = ContextType.Domain;
        public PrincipalContext Context { get; protected set; }
        public UserPrincipal User { get; protected set; }
        public UserPrincipal Manager { get; protected set; }
        public bool IsManager { get; protected set; }
        public List<UserPrincipal> DirectReports { get; protected set; }

        public class AuthenticationResult
        {
            public AuthenticationResult()
            {
                IdentityError = new List<IdentityError>();
                IsSuccess = IdentityError.Count > 0;
            }
            public List<IdentityError> IdentityError { get; private set; }
            public String RoleName { get; private set; }
            public Boolean IsSuccess { get; set; }
            public ManageDirectoryServices Context { get; set; }
        }

        public ManageDirectoryServices()
        {
            Context = new PrincipalContext(contextType);
            DirectReports = new List<UserPrincipal>();
        }

        public ManageDirectoryServices(string ntid)
        {
            Context = new PrincipalContext(contextType);
            DirectReports = new List<UserPrincipal>();
            GetEmployeeByNTID(NormalizeNTID(ntid));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ntid">This is SamAccountName</param>
        /// <returns></returns>
        public ManageDirectoryServices GetEmployeeByNTID(string ntid)
        {
            if (string.IsNullOrWhiteSpace(ntid)) return null;
            UserPrincipal searchTemplate = new UserPrincipal(Context)
            {
                SamAccountName = ntid
            };
            PrincipalSearcher ps = new PrincipalSearcher(searchTemplate);
            User = (UserPrincipal)ps.FindOne();
            return this;
        }


        public ManageDirectoryServices GetEmployee(string strSearch, string prop)
        {
            if (string.IsNullOrWhiteSpace(strSearch)) return this;
            if (string.IsNullOrWhiteSpace(prop)) return this;

            DirectorySearcher search = new DirectorySearcher();
            search.Filter = String.Format("(cn={0})", strSearch);
            search.PropertiesToLoad.Add(prop);
            var result = search.FindAll();

            if (result != null)
            {
                int directReports = result.Count; //result.Properties["displayname"].Count;
                if (directReports < 0) return null;

                for (int counter = 0; counter < directReports; counter++)
                {
                    var user = (string)result[counter].Properties["givenname"][counter];
                    var reporte = UserPrincipal.FindByIdentity(Context, IdentityType.DistinguishedName, user);
                    this.DirectReports.Add(reporte);
                    IsManager = true;
                }
                return this;
            }
            return null;
        }



        public ManageDirectoryServices GetEmployee(UserPrincipal searchTemplate)
        {
            if (searchTemplate == null) return null;
            PrincipalSearcher ps = new PrincipalSearcher(searchTemplate);
            User = (UserPrincipal)ps.FindOne();
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="NTID">This is SamAccountName</param>
        /// <returns></returns>
        public bool IsUserExist(string NTID)
        {
            var data = GetEmployeeByNTID(NormalizeNTID(NTID));
            return !string.IsNullOrWhiteSpace(data?.User?.SamAccountName);
        }

        public bool IsUserExist()
        {
            var data = User;
            return !string.IsNullOrWhiteSpace(data?.SamAccountName);
        }

        public ManageDirectoryServices GetEmployeeByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            UserPrincipal searchTemplate = new UserPrincipal(Context)
            {
                EmailAddress = email
            };
            PrincipalSearcher ps = new PrincipalSearcher(searchTemplate);
            User = (UserPrincipal)ps.FindOne();
            return this;
        }

        public ManageDirectoryServices GetEmployeeByEmpId(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId)) return null;
            UserPrincipal searchTemplate = new UserPrincipal(Context)
            {
                EmployeeId = employeeId
            };
            PrincipalSearcher ps = new PrincipalSearcher(searchTemplate);
            User = (UserPrincipal)ps.FindOne();
            return this;
        }

        public ManageDirectoryServices GetManager()
        {
            if (this.User == null) return null;
            DirectoryEntry ManagerDE = this.User.GetUnderlyingObject() as DirectoryEntry;
            var manager = ManagerDE.Properties["manager"].Value.ToString();
            UserPrincipal oManager = UserPrincipal.FindByIdentity(Context, IdentityType.DistinguishedName, manager);
            this.Manager = oManager;
            return this;
        }

        public ManageDirectoryServices GetDirectReports()
        {
            if (this.User == null) return this;
            DirectorySearcher search = new DirectorySearcher();
            search.Filter = String.Format("(cn={0})", this.User.SamAccountName);
            search.PropertiesToLoad.Add("directReports");
            SearchResult result = search.FindOne();
            if (result != null)
            {
                int directReports = result.Properties["directReports"].Count;
                if (directReports < 0) return null;

                for (int counter = 0; counter < directReports; counter++)
                {
                    var user = (string)result.Properties["directReports"][counter];
                    var reporte = UserPrincipal.FindByIdentity(Context, IdentityType.DistinguishedName, user);
                    this.DirectReports.Add(reporte);
                    IsManager = true;
                }
                return this;
            }
            return null;
        }

        public string NormalizeNTID(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id)) return "";
            return Id.Trim().ToUpper().Replace(@"\", "")
                .Replace("\\", "")
                .Replace("/", "")
                .Replace("//", "")
                .Replace("MS", "")
                .Replace("MS//", "")
                .Replace("MS\\", "");
        }

        public AuthenticationResult SignIn(string ntid, string password)
        {
            var NormalizeNTID = this.NormalizeNTID(ntid);
            bool IsAuthenticated = false;
            IdentityError identityError = new IdentityError();
            ManageDirectoryServices context = null;
            AuthenticationResult authenticationResult = new AuthenticationResult();
            var IsSuccess = Context.ValidateCredentials(NormalizeNTID, password, ContextOptions.Negotiate);
            context = GetEmployeeByNTID(NormalizeNTID);
            if (IsSuccess)
            {
                if (context.User != null)
                {
                    IsAuthenticated = true;
                    this.User = context.User;
                    authenticationResult.Context = context;
                    authenticationResult.IsSuccess = true;
                }
            }
            else
            {
                if (!IsAuthenticated || User == null)
                {
                    authenticationResult.IdentityError.Add(new IdentityError
                    {
                        Code = "InCorrectUserAndPassword",
                        Description = "Username or Password is not correct"
                    });
                }

                if (context.User.IsAccountLockedOut())
                {
                    authenticationResult.IdentityError.Add(new IdentityError
                    {
                        Code = "YourAccountIsLocked",
                        Description = "Your account is locked."
                    });
                }

                if (context.User.Enabled.HasValue && User.Enabled.Value == false)
                {
                    authenticationResult.IdentityError.Add(identityError = new IdentityError
                    {
                        Code = "YourAccountIsDisabled",
                        Description = "Your account is disabled"
                    });
                }
                else
                {
                    authenticationResult.IdentityError.Add(new IdentityError
                    {
                        Code = "InvalidLogin",
                        Description = "In valid login!! Please try again"
                    });
                }
            }
            return authenticationResult;
        }

        #region ************Async Envelope**************

        public async Task<ManageDirectoryServices> GetEmployeeByNTIDAsync(string ntid)
        {
            return await Task.Run(() => GetEmployeeByNTID(ntid));
        }

        public async Task<ManageDirectoryServices> GetEmployeeByEmailAsync(string email)
        {
            return await Task.Run(() => GetEmployeeByEmail(email));
        }

        public async Task<ManageDirectoryServices> GetEmployeeByEmpIdAsync(string employeeId)
        {
            return await Task.Run(() => GetEmployeeByEmpId(employeeId));
        }

        public async Task<ManageDirectoryServices> GetManagerAsync()
        {
            return await Task.Run(() => GetManager());
        }

        public async Task<ManageDirectoryServices> GetDirectReportsAsync()
        {
            return await Task.Run(() => GetDirectReports());
        }

        public async Task<AuthenticationResult> SignInAsync(string ntid, string password)
        {
            return await Task.Run(() => SignIn(ntid, password));
        }

        #endregion

        public void Dispose()
        {
            this.Dispose();
            GC.Collect();
        }

    }
}
