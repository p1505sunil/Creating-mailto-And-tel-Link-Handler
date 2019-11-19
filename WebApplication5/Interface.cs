public class MailWebLink : WebLink
{
    #region Prefix  
    protected static string LinkPrefix { get { return "mailto:"; } }
    public override string Prefix => LinkPrefix;
    #endregion

    #region Delimiters  
    protected static readonly char[] MailDelimiters = new char[] { '?' };
    protected static readonly char[] RecipientDelimiters = new char[] { ',', ';' };
    protected static readonly char[] ParamDelimiters = new char[] { '&' };
    protected static readonly char[] ParamValueDelimiters = new char[] { '=' };
    #endregion  

    #region Field Names  
    protected static readonly string ToField = "to";
    protected static readonly string CcField = "cc";
    protected static readonly string BccField = "bcc";
    protected static readonly string SubjectField = "subject";
    protected static readonly string BodyField = "body";
    #endregion


    #region Fields  
    public string[] To { get; set; }
    public string[] Cc { get; set; }
    public string[] Bcc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    #endregion

    public MailWebLink()
    {

    }
    public MailWebLink(string link)
    {
        ReadLink(link);
    }

    public static bool CanHandle(string link)
    {
        return link.ToLower().Trim().StartsWith(LinkPrefix);
    }

    #region Link Loading  
    public override void ClearFields()
    {
        To = Cc = Bcc = null;
        Subject = Body = null;
    }

    public override void ReadLink(string link)
    {
        base.ReadLink(link);

        try
        {
            ClearFields();

            // Exclude prefix if necessary  
            link = ExcludePrefix(link);

            // Get mail 'To' Field  
            string tmpVal = null;
            int idx = -1;

            idx = link.IndexOfAny(MailDelimiters);

            if (idx > -1)
                tmpVal = link.Substring(0, idx);
            else
                tmpVal = link;

            this.To = LoadRecipients(tmpVal).ToArray();

            if (idx == -1)
                return;

            link = link.Substring(idx + 1);

            // Handle rest of fields  
            var parameters = GetParameters(link, true);
            foreach (var par in parameters)
            {
                if (par.Key == ToField) // overrides the above code  
                    this.To = LoadRecipients(par.Value).ToArray();
                else if (par.Key == CcField)
                    this.Cc = LoadRecipients(par.Value).ToArray();
                else if (par.Key == BccField)
                    this.Bcc = LoadRecipients(par.Value).ToArray();
                else if (par.Key == SubjectField)
                    this.Subject = par.Value;
                else if (par.Key == BodyField)
                    this.Body = par.Value;
            }
        }
        catch
        {
            throw new FormatException();
        }
    }

    /// <summary>  
    /// Splits a mail string into a list of mail addresses.  
    /// </summary>  
    protected virtual IEnumerable<string> LoadRecipients(string val)
    {
        var items = val.Split(RecipientDelimiters, StringSplitOptions.RemoveEmptyEntries);
        return items.Select(s => s.Trim().ToLower()).Distinct();
    }

    /// <summary>  
    /// Splits a parameter string into a list of parameters (kay and value)  
    /// </summary>  
    /// <param name="skipEmpty">Whether to skip empty parameters.</param>  
    protected virtual IEnumerable<KeyValuePair<string, string>> GetParameters(string val, bool skipEmpty = true)
    {
        var items = val.Split(ParamDelimiters, StringSplitOptions.RemoveEmptyEntries);

        foreach (var itm in items)
        {
            string key = string.Empty;
            string value = string.Empty;

            var delimiterIdx = itm.IndexOfAny(ParamValueDelimiters);
            if (delimiterIdx == -1)
                continue;

            key = itm.Substring(0, delimiterIdx).ToLower();
            value = itm.Substring(delimiterIdx + 1);
            value = UnscapeParamValue(value);

            if (key.Length == 0)
                continue;

            if (skipEmpty && value.Length == 0)
                continue;

            yield return new KeyValuePair<string, string>(key, value);
        }
    }
    #endregion

    #region Link Generation  

    public virtual string GetLink() { return GenerateLink(true); }

    public override string GenerateLink(bool includePrefix)
    {
        string str = base.GenerateLink(includePrefix);

        if (this.To != null && this.To.Length > 0)
        {
            str += GetRecipientString(this.To);
        }

        str += MailDelimiters.First();

        if (this.Cc != null && this.Cc.Length > 0)
        {
            str += GetParameterString(CcField, GetRecipientString(this.Cc), false);
            str += ParamDelimiters.First();
        }

        if (this.Bcc != null && this.Bcc.Length > 0)
        {
            str += GetParameterString(BccField, GetRecipientString(this.Bcc), false);
            str += ParamDelimiters.First();
        }

        if (this.Subject != null && this.Subject.Length > 0)
        {
            str += GetParameterString(SubjectField, this.Subject, true);
            str += ParamDelimiters.First();
        }

        if (this.Body != null && this.Body.Length > 0)
        {
            str += GetParameterString(BodyField, this.Body, true);
            str += ParamDelimiters.First();
        }

        str = str.TrimEnd(MailDelimiters.Concat(ParamDelimiters).ToArray());

        return str;
    }

    /// <summary>  
    /// Joins a list of mail addresses into a string  
    /// </summary>  
    protected virtual string GetRecipientString(string[] recipients)
    {
        return string.Join(RecipientDelimiters.First().ToString(), recipients);
    }

    /// <summary>  
    /// Joins a parameter (key and value) into a string  
    /// </summary>  
    /// <param name="escapeValue">Whether to escape value.</param>  
    protected virtual string GetParameterString(string key, string value, bool escapeValue)
    {
        return string.Format("{0}{1}{2}",
          key,
          ParamValueDelimiters.First(),
          escapeValue ? EscapeParamValue(value) : value);
    }

    #endregion

    #region Helpers  
    protected static readonly Dictionary<string, string> CustomUnescapeCharacters =
      new Dictionary<string, string>() { { "+", " " } };

    private static string EscapeParamValue(string value)
    {
        return Uri.EscapeDataString(value);
    }

    private static string UnscapeParamValue(string value)
    {
        foreach (var customChar in CustomUnescapeCharacters)
        {
            if (value.Contains(customChar.Key))
                value = value.Replace(customChar.Key, customChar.Value);
        }

        return Uri.UnescapeDataString(value);
    }
    #endregion
}