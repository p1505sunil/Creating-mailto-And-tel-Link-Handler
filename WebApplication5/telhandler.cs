public class TelephoneWebLink : WebLink
{
    #region Prefix  
    protected static string LinkPrefix { get { return "tel:"; } }
    public override string Prefix => LinkPrefix;
    #endregion

    #region Delimiters  
    protected static readonly char ExtensionDelimiter = 'p';
    #endregion

    #region Fields  
    public string Number { get; set; }
    public string Extension { get; set; }
    #endregion


    public TelephoneWebLink()
    {

    }
    public TelephoneWebLink(string link)
    {
        ReadLink(link);
    }

    public static bool CanHandle(string link)
    {
        return link.ToLower().Trim().StartsWith(LinkPrefix);
    }

    public override void ClearFields()
    {
        Number = null;
        Extension = null;
    }

    public override void ReadLink(string link)
    {
        base.ReadLink(link);

        try
        {
            ClearFields();

            // Exclude prefix if necessary  
            link = ExcludePrefix(link).Trim();

            Number = string.Empty;
            Extension = string.Empty;

            bool foundExtension = false;
            int idx = 0;
            foreach (var c in link)
            {
                if (idx == 0 && c == '+')
                    Number += "+";
                if (c == ExtensionDelimiter)
                    foundExtension = true;
                else if (char.IsDigit(c))
                {
                    if (foundExtension == false)
                        Number += c.ToString();
                    else
                        Extension += c.ToString();
                }
                idx++;
            }

        }
        catch
        {
            throw new FormatException();
        }
    }

    public override string GenerateLink(bool includePrefix)
    {
        var str = base.GenerateLink(includePrefix);

        if (Number != null)
            str += Number.ToString();

        if (Extension != null && Extension.Length > 0)
            str += ExtensionDelimiter.ToString() + Extension;

        return str;
    }
}