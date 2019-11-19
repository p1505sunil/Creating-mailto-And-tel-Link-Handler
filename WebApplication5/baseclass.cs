public abstract class WebLink
{
    /// <summary>  
    /// Link prefix. Examples are: 'mailto:' and 'tel:'  
    /// </summary>  
    public abstract string Prefix { get; }

    /// <summary>  
    /// Clears instance fields.  
    /// </summary>  
    public abstract void ClearFields();

    /// <summary>  
    /// Loads link input into relevant fields.  
    /// </summary>  
    public virtual void ReadLink(string link)
    {
        if (link == null)
            throw new ArgumentNullException("link");

        if (link.ToLower().StartsWith(Prefix.ToLower()) == false)
            throw new FormatException("Invalid link.");
    }

    /// <summary>  
    /// Generates link from instance fields.  
    /// </summary>  
    public virtual string GenerateLink(bool includePrefix)
    {
        var str = string.Empty;

        if (includePrefix)
            str += Prefix;

        return str;
    }

    /// <summary>  
    /// Can be used to exclude prefix from a link string.  
    /// </summary>  
    protected string ExcludePrefix(string link)
    {
        link = link.Trim();
        if (link.ToLower().StartsWith(Prefix.ToLower()))
            link = link.Substring(Prefix.Length).Trim();
        return link;
    }

    public override string ToString()
    {
        return GenerateLink(true);
    }
}