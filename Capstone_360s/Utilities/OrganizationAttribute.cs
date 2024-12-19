namespace Capstone_360s.Utilities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OrganizationAttribute : Attribute
    {
        public string OrganizationName { get; }

        public OrganizationAttribute(string organizationName)
        {
            OrganizationName = organizationName;
        }
    }
}
