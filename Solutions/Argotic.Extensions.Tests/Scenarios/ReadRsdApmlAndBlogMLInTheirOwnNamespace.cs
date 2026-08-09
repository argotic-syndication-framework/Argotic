namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Reading RSD, APML and BlogML documents that declare the format's own namespace as their default.
/// </summary>
/// <remarks>
///     <para>
///     Each of RSD, APML and BlogML resolves its prefixed XPath through a
///     <see cref="XmlNamespaceManager"/> built by its own utility class, and each of those built the binding
///     with <c>AddNamespace(prefix, !string.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace
///     : CONSTANT)</c> two lines after constructing a fresh manager.
///     </para>
///     <para>
///     The conditional could never take its first arm. <see cref="XmlNamespaceManager.DefaultNamespace"/>
///     returns "the namespace URI for the default namespace, or an empty string if there is no default
///     namespace", and the only way to acquire one is <c>AddNamespace(String.Empty, …)</c> — verified at
///     <c>System.Xml.ReaderWriter.xml:4914-4917</c> and <c>:4864-4868</c>. The manager is built from an
///     <see cref="XmlNameTable"/>, an atomised <i>string</i> table that carries no prefix bindings, one line
///     earlier. So the constant arm was taken every time, and deleting the conditional is observationally
///     inert — no input distinguishes before from after, and these tests are green on both sides.
///     </para>
///     <para>
///     They are not a ritual. The conditional was not merely dead but wrong in intent: binding a format's
///     prefix to whatever default namespace a document happened to declare would make <i>any</i> document
///     parse as though it were RSD, APML or BlogML. These pin the binding to each format's own constant, so
///     that resurrecting that intent has to break a named test.
///     </para>
/// </remarks>
[TestClass]
public class ReadRsdApmlAndBlogMLInTheirOwnNamespace
{
    /// <summary>
    /// An RSD document declaring <c>http://archipelago.phrasewise.com/rsd</c> is read as RSD.
    /// </summary>
    [TestMethod]
    public void AnRsdDocumentInTheRsdNamespace_IsReadAsRsd()
    {
        const string Xml = """
            <?xml version="1.0"?>
            <rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
                <service>
                    <engineName>Namespaced Engine</engineName>
                    <engineLink>http://example.com</engineLink>
                    <homePageLink>http://example.com/blog</homePageLink>
                    <apis>
                        <api name="MetaWeblog" preferred="true" apiLink="http://example.com/xmlrpc" blogID="1">
                            <settings>
                                <docs>http://example.com/docs</docs>
                            </settings>
                        </api>
                    </apis>
                </service>
            </rsd>
            """;

        RsdDocument document = new();
        using XmlReader reader = XmlReader.Create(new StringReader(Xml));
        document.Load(reader);

        document.EngineName.ShouldBe("Namespaced Engine");
        document.Interfaces.Count.ShouldBe(1);
        document.Interfaces[0].Name.ShouldBe("MetaWeblog");
        document.Interfaces[0].Documentation.ShouldBe(new Uri("http://example.com/docs"));
    }

    /// <summary>
    /// An APML document declaring <c>http://www.apml.org/apml-0.6</c> is read as APML.
    /// </summary>
    [TestMethod]
    public void AnApmlDocumentInTheApmlNamespace_IsReadAsApml()
    {
        const string Xml = """
            <?xml version="1.0"?>
            <APML xmlns="http://www.apml.org/apml-0.6" version="0.6">
                <Head>
                    <Title>Namespaced Profile</Title>
                </Head>
                <Body defaultprofile="Home">
                    <Profile name="Home">
                        <ImplicitData>
                            <Concepts>
                                <Concept key="syndication" value="0.85" from="Example" />
                            </Concepts>
                        </ImplicitData>
                    </Profile>
                </Body>
            </APML>
            """;

        ApmlDocument document = new();
        using XmlReader reader = XmlReader.Create(new StringReader(Xml));
        document.Load(reader);

        document.Head.Title.ShouldBe("Namespaced Profile");
        document.Profiles.Count.ShouldBe(1);
        document.Profiles[0].ImplicitConcepts.Count.ShouldBe(1);
        document.Profiles[0].ImplicitConcepts[0].Key.ShouldBe("syndication");
        document.Profiles[0].ImplicitConcepts[0].Value.ShouldBe(0.85m);
    }

    /// <summary>
    /// A BlogML document declaring <c>http://www.blogml.com/2006/09/BlogML</c> is read as BlogML.
    /// </summary>
    [TestMethod]
    public void ABlogMLDocumentInTheBlogMLNamespace_IsReadAsBlogML()
    {
        const string Xml = """
            <?xml version="1.0"?>
            <blog root-url="http://example.com/blog" date-created="2024-01-01T08:00:00Z" xmlns="http://www.blogml.com/2006/09/BlogML">
                <title type="text">Namespaced Blog</title>
                <sub-title type="text">A subtitle</sub-title>
                <authors>
                    <author id="a1" email="ada@example.com" title="Ada" />
                </authors>
                <categories>
                    <category id="c1" parentref="0" title="General" />
                </categories>
                <posts>
                    <post id="p1" date-created="2024-01-02T09:00:00Z" post-url="http://example.com/blog/p1" type="normal" hasexcerpt="false">
                        <title type="text">First post</title>
                        <content type="text">Hello</content>
                    </post>
                </posts>
            </blog>
            """;

        BlogMLDocument document = new();
        using XmlReader reader = XmlReader.Create(new StringReader(Xml));
        document.Load(reader);

        document.Title.ShouldNotBeNull().Content.ShouldBe("Namespaced Blog");
        document.RootUrl.ShouldBe(new Uri("http://example.com/blog"));
        document.Authors.Count.ShouldBe(1);
        document.Categories.Count.ShouldBe(1);
        document.Posts.Count.ShouldBe(1);
        document.Posts[0].Title.ShouldNotBeNull().Content.ShouldBe("First post");
    }
}