using Argotic.Net;
namespace Argotic.Extensions.Tests.Functionality.Core.Net.XmlRpc;

/// <summary>
/// Covers the depth bound on the XML-RPC composite parser.
/// </summary>
/// <remarks>
///     <para>
///     Two cycles re-enter <c>XmlRpcClient.TryParseValue</c>, neither of them tail-recursive: an array
///     costs two stack frames per nesting level and a structure three. Nothing bounded either, so a
///     hostile or broken server could reply with a deeply nested payload and overflow the stack of the
///     process reading it — a process kill, not a catchable exception. It is reachable from the
///     network at trivial cost: a nesting level is 43 bytes of document for an array and 49 for a
///     structure (no <c>name</c> element is required), against the 2 MiB body allowance
///     <c>XmlRpcResponse.CreateAsync</c> already permits.
///     </para>
///     <para>
///     <b>Nothing here overflows a stack on purpose.</b> A test that did would take the test host with
///     it, which is precisely the defect. These test at the cap, one level past it, and at two hundred
///     levels — about 8.6 KB of document, which the unbounded parser walks without difficulty.
///     </para>
///     <para>
///     The two public <see cref="XPathNodeIterator"/> constructors are covered separately: they
///     recurse through the same cycle, and a cap applied only to <c>Load</c> would leave them open.
///     </para>
/// </remarks>
[TestClass]
public sealed class XmlRpcValueNestingDepthTests
{
    /// <summary>
    /// The cap these tests were written against.
    /// </summary>
    private const int Cap = 64;

    /// <summary>
    /// Builds a chain of <paramref name="levels"/> nested arrays with one integer at the innermost level.
    /// </summary>
    /// <param name="levels">The number of nested <c>array</c> elements.</param>
    /// <returns>The <c>value</c> element markup.</returns>
    private static string ArrayChain(int levels)
    {
        StringBuilder builder = new(capacity: 64 + (levels * 48));

        for (int level = 0; level < levels; level++)
        {
            builder.Append("<value><array><data>");
        }

        builder.Append("<value><i4>1138</i4></value>");

        for (int level = 0; level < levels; level++)
        {
            builder.Append("</data></array></value>");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds a chain of <paramref name="levels"/> nested structures with one integer at the innermost level.
    /// </summary>
    /// <param name="levels">The number of nested <c>struct</c> elements.</param>
    /// <returns>The <c>value</c> element markup.</returns>
    private static string StructureChain(int levels)
    {
        StringBuilder builder = new(capacity: 64 + (levels * 56));

        for (int level = 0; level < levels; level++)
        {
            builder.Append("<value><struct><member><name>m</name>");
        }

        builder.Append("<value><i4>1138</i4></value>");

        for (int level = 0; level < levels; level++)
        {
            builder.Append("</member></struct></value>");
        }

        return builder.ToString();
    }

    private static XPathNavigator Value(string xml) =>
        new XPathDocument(new StringReader(xml)).CreateNavigator()!.SelectSingleNode("//value")!;

    /// <summary>
    /// Counts how many nested <see cref="XmlRpcArrayValue"/> levels a parsed value actually carries.
    /// </summary>
    /// <param name="value">The parsed value at the outermost level.</param>
    /// <returns>The number of array levels reached.</returns>
    private static int ArrayLevelsIn(IXmlRpcValue? value)
    {
        int levels = 0;

        while (value is XmlRpcArrayValue array && array.Values.Count > 0)
        {
            levels++;
            value = array.Values[0];
        }

        return levels;
    }

    /// <summary>
    /// Counts how many nested <see cref="XmlRpcStructureValue"/> levels a parsed value actually carries.
    /// </summary>
    /// <param name="value">The parsed value at the outermost level.</param>
    /// <returns>The number of structure levels reached.</returns>
    private static int StructureLevelsIn(IXmlRpcValue? value)
    {
        int levels = 0;

        while (value is XmlRpcStructureValue structure && structure.Members.Count > 0)
        {
            levels++;
            value = structure.Members[0].Value;
        }

        return levels;
    }

    /// <summary>
    /// The cap is sixty-four, and every assertion below is written against that number.
    /// </summary>
    /// <remarks>
    ///     Pinned because <c>MaxValueNestingDepth</c> is a <c>public const</c>: its value is inlined
    ///     into every assembly ever compiled against this one, so changing it is a binary break as well
    ///     as a change of behaviour, and should have to be argued for rather than typed. The bound
    ///     itself is generous — XML-RPC's own vocabulary nests two or three levels, and the deepest
    ///     payload in this suite is three.
    /// </remarks>
    [TestMethod]
    public void TheCap_IsSixtyFour()
    {
        XmlRpcClient.MaxValueNestingDepth.ShouldBe(Cap);
    }

    /// <summary>
    /// An array chain nested exactly to the cap parses to its full depth.
    /// </summary>
    /// <remarks>
    ///     The boundary from the accepting side, and green both before and after. Without it a cap one
    ///     level too tight would be indistinguishable from a correct one.
    /// </remarks>
    [TestMethod]
    public void AnArrayChainAtTheCap_ParsesToItsFullDepth()
    {
        XmlRpcClient.TryParseValue(Value(ArrayChain(Cap)), out IXmlRpcValue? value).ShouldBeTrue();

        ArrayLevelsIn(value).ShouldBe(Cap, "INVARIANT: the cap admits exactly as many levels as it names");
    }

    /// <summary>
    /// An array chain one level past the cap is refused outright.
    /// </summary>
    /// <remarks>
    ///     <c>XmlRpcArrayValue.Load</c> reports success only if at least one element parsed, and a chain
    ///     has exactly one element per level, so the refusal at the bottom propagates all the way back
    ///     up. The whole document is rejected rather than silently truncated — which for a payload
    ///     shaped like this is the honest answer.
    /// </remarks>
    [TestMethod]
    public void AnArrayChainOneLevelPastTheCap_IsRefused()
    {
        XmlRpcClient.TryParseValue(Value(ArrayChain(Cap + 1)), out IXmlRpcValue? value)
            .ShouldBeFalse("INVERTED: one level too many is refused rather than followed");

        value.ShouldBeNull();
    }

    /// <summary>
    /// A two-hundred-level array chain is refused rather than walked.
    /// </summary>
    /// <remarks>
    ///     The shape that would eventually kill the process. Two hundred levels is far below any stack
    ///     limit, so this measures the bound rather than the stack — the payload that actually
    ///     overflows is around four thousand levels, roughly 176 KB, under a tenth of what the
    ///     content-length limit already lets through.
    /// </remarks>
    [TestMethod]
    public void ADeepArrayChain_IsRefusedRatherThanWalked()
    {
        XmlRpcClient.TryParseValue(Value(ArrayChain(200)), out IXmlRpcValue? value)
            .ShouldBeFalse("INVERTED: depth is bounded by a constant, not by the stack");

        ArrayLevelsIn(value).ShouldBe(0);
    }

    /// <summary>
    /// A structure chain nested exactly to the cap parses to its full depth.
    /// </summary>
    [TestMethod]
    public void AStructureChainAtTheCap_ParsesToItsFullDepth()
    {
        XmlRpcClient.TryParseValue(Value(StructureChain(Cap)), out IXmlRpcValue? value).ShouldBeTrue();

        StructureLevelsIn(value).ShouldBe(Cap, "INVARIANT: the three-frame cycle admits the same depth");
    }

    /// <summary>
    /// A deep structure chain stops at the cap.
    /// </summary>
    /// <remarks>
    ///     The structure cycle is three frames per level and runs through
    ///     <c>XmlRpcStructureMember.Load</c>, a third method the cap has to reach. It stops rather than
    ///     cascading because a member with a readable <c>name</c> counts as loaded whatever happens to
    ///     its value — so the refusal is contained at the level that hit the cap, and everything above
    ///     it survives.
    /// </remarks>
    [TestMethod]
    public void ADeepStructureChain_StopsAtTheCap()
    {
        XmlRpcClient.TryParseValue(Value(StructureChain(200)), out IXmlRpcValue? value).ShouldBeTrue();

        StructureLevelsIn(value).ShouldBe(Cap, "INVERTED: recursion stops at the cap, not at the document's end");
    }

    /// <summary>
    /// The public <see cref="XmlRpcArrayValue(XPathNodeIterator)"/> constructor is bounded as well.
    /// </summary>
    /// <remarks>
    ///     The first of the two doors a cap applied only to <c>Load</c> would leave open. It recurses
    ///     through exactly the same cycle and is reachable by any consumer holding an iterator.
    /// </remarks>
    [TestMethod]
    public void TheArrayIteratorConstructor_IsBoundedToo()
    {
        XPathNodeIterator values = Value(ArrayChain(200)).Select("array/data/value");

        XmlRpcArrayValue array = new(values);

        array.Values.ShouldBeEmpty("INVERTED: the public constructor honours the same cap as Load");

        // `Values` is [] before the constructor body runs, so the assertion above is satisfied by a
        // constructor that ignored its iterator entirely — and by a cap of 0, or of 1. The accepting
        // side is what makes the refusal mean something: a payload within the cap must produce a
        // non-empty, non-default result through the same constructor.
        XmlRpcArrayValue withinTheCap = new(Value(ArrayChain(4)).Select("array/data/value"));
        withinTheCap.Values.ShouldNotBeEmpty("a payload well inside the cap must still be read");
    }

    /// <summary>
    /// The public <see cref="XmlRpcStructureValue(XPathNodeIterator)"/> constructor is bounded as well.
    /// </summary>
    [TestMethod]
    public void TheStructureIteratorConstructor_IsBoundedToo()
    {
        XPathNodeIterator members = Value(StructureChain(200)).Select("struct/member");

        XmlRpcStructureValue structure = new(members);

        StructureLevelsIn(structure).ShouldBe(
            Cap,
            "INVERTED: the public constructor honours the same cap as Load");
    }

    /// <summary>
    /// The shapes XML-RPC actually uses are nowhere near the cap.
    /// </summary>
    /// <remarks>
    ///     The control that says the cap is not a functional restriction. A
    ///     <c>metaWeblog.getRecentPosts</c> response is an array of structures of scalars — three
    ///     levels — and nothing else in this suite nests past two.
    /// </remarks>
    [TestMethod]
    public void AnOrdinaryThreeLevelPayload_IsUnaffectedByTheCap()
    {
        const string Xml = """
            <value><array><data>
              <value><struct>
                <member><name>title</name><value><string>A post</string></value></member>
                <member><name>tags</name><value><array><data>
                  <value><string>dotnet</string></value>
                </data></array></value></member>
              </struct></value>
            </data></array></value>
            """;

        XmlRpcClient.TryParseValue(Value(Xml), out IXmlRpcValue? value).ShouldBeTrue();

        XmlRpcArrayValue posts = value.ShouldBeOfType<XmlRpcArrayValue>();
        XmlRpcStructureValue post = posts.Values[0].ShouldBeOfType<XmlRpcStructureValue>();
        post.Members.Count.ShouldBe(2);
        post["tags"]!.Value.ShouldBeOfType<XmlRpcArrayValue>().Values.Count.ShouldBe(1);
    }
}