using System.DirectoryServices;

namespace Airlines.XAirlines.Dialogs
{
    public static class ActiveDirectoryHelper
    {

        public static string GetCrewId(string userName)
        {
            using (DirectoryEntry dse = new DirectoryEntry("LDAP://RootDSE"))
            {

                string ldap = "LDAP://" + dse.Properties["defaultNamingContext"][0].ToString();
                using (DirectoryEntry de = new DirectoryEntry(ldap))
                {
                   using( DirectorySearcher ds = new DirectorySearcher(de))
                        {
                        ds.PropertiesToLoad.Add("ExtensionAttribute1");

                        ds.Filter = "(&(objectCategory=User)(objectClass=person)(sAMAccountName = " + userName + " *))";

                        SearchResult result = ds.FindOne();
                        return result.Properties["ExtensionAttribute1"][0].ToString();
                    }
                  
                }


            }

        }
    }
}
