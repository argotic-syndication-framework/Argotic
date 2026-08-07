using System.Collections;
using System.Reflection;
using Argotic.Extensions.Core;
using Argotic.Net;
using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

// As in ComparisonContractTests: a throwing constructor, setter or comparison marks a shape the attack
// cannot reach, and the sweep must continue past it rather than abort at the first one.
#pragma warning disable CA1031

// The randomised sweep needs a fixed seed so a failure reproduces; System.Random is the right
// instrument and no security decision depends on it.
#pragma warning disable CA5394

/// <summary>
/// Hand-built attacks on the members changed this week, where a generated population cannot reach
/// the shape that breaks the contract.
/// </summary>
[TestClass]
public class ComparisonContractAttackTests
{
    private static readonly string ReportDirectory =
        Environment.GetEnvironmentVariable("ARGOTIC_CONTRACT_REPORT_DIR") ?? Path.GetTempPath();

    private static void Report(string name, IEnumerable<string> lines)
    {
        try
        {
            File.WriteAllLines(Path.Combine(ReportDirectory, $"attack-{name}.txt"), lines);
        }
        catch (IOException)
        {
            // The assertion below is the verdict; the file is a convenience.
        }
    }

    /// <summary>
    /// For every comparable type, reports which members <c>CompareTo</c> consults and which
    /// <c>GetHashCode</c> folds. A member seen by the hash but not the comparison is a lookup miss;
    /// the reverse is only a collision.
    /// </summary>
    [TestMethod]
    public void MemberSetsSeenByCompareAndHashMustNotDisagreeInTheDangerousDirection()
    {
        List<string> matrix = [];
        List<string> misses = [];
        List<string> collisions = [];

        foreach (Type type in ComparisonContractHarness.ComparableTypes())
        {
            if (!ComparisonContractHarness.HasSelfCompareTo(type))
            {
                continue;
            }

            object? baseline = ComparisonContractHarness.TryCreateDefault(type);
            if (baseline is null)
            {
                matrix.Add($"{type.FullName}\tUNCONSTRUCTIBLE");
                continue;
            }

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.GetIndexParameters().Length == 0)
                         .OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                bool anyCompared = false;
                bool anyHashed = false;
                bool probed = false;

                // Every candidate value is tried, not just the first. Probing a nullable member with
                // its zero value only would report "not hashed" for every one of them, because null
                // and zero hash alike -- a weakness that would have hidden a real miss behind it.
                foreach (object mutated in Mutations(type, property))
                {
                    object? reference = ComparisonContractHarness.TryCreateDefault(type);
                    if (reference is null)
                    {
                        continue;
                    }

                    bool compared;
                    try
                    {
                        compared = ComparisonContractHarness.Compare(type, reference, mutated) != 0;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    probed = true;
                    bool hashed = reference.GetHashCode() != mutated.GetHashCode();
                    anyCompared |= compared;
                    anyHashed |= hashed;

                    if (hashed && !compared)
                    {
                        misses.Add($"MISS {type.FullName}.{property.Name}: a value that compares equal to the default hashes differently ({Render(property.GetValue(mutated))})");
                    }
                }

                if (!probed)
                {
                    continue;
                }

                matrix.Add($"{type.FullName}.{property.Name}\tcompared={anyCompared}\thashed={anyHashed}");
                if (anyCompared && !anyHashed)
                {
                    collisions.Add($"COST {type.FullName}.{property.Name}: no candidate value that changes the comparison changes the hash");
                }
            }
        }

        Report("member-matrix", matrix);
        Report("member-misses", misses);
        Report("member-collisions", collisions);

        // Without this floor the sweep reports success having probed nothing: every member reaching
        // `if (!probed) continue;` leaves `misses` empty, and ShouldBeEmpty is satisfied. The measured
        // figure is 490 probed members; 300 leaves room for the object model to move without letting a
        // collapse of the mutation generator pass.
        matrix.Count.ShouldBeGreaterThanOrEqualTo(
            300,
            $"only {matrix.Count} members were probed, so an empty `misses` says nothing");

        misses.ShouldBeEmpty($"{misses.Count} members are hashed but not compared:\n{string.Join("\n", misses.Take(40))}");
    }

    private static string SortOutcome(Action sort)
    {
        try
        {
            sort();
            return "no throw";
        }
        catch (InvalidOperationException ex)
        {
            return $"{ex.GetType().Name}: {ex.Message} (inner {ex.InnerException?.GetType().Name})";
        }
    }

    /// <summary>
    /// Brute-forces the ordering rule the widened <see cref="SitemapVideo"/> comparison uses for URIs.
    /// <see cref="Uri.Compare"/> switches rule when either operand is relative — comparing the original
    /// strings rather than the unescaped absolute URI — and two rules in one ordering is where
    /// transitivity usually goes.
    /// </summary>
    [TestMethod]
    public void TheUriOrderingRuleMustBeTransitiveAcrossRelativeAndEscapedForms()
    {
        List<Uri> pool =
        [
            new Uri("http://example.com/a"),
            new Uri("HTTP://EXAMPLE.COM/A"),
            new Uri("http://example.com/a%2Fb"),
            new Uri("http://example.com/a/b"),
            new Uri("http://example.com/a%20b"),
            new Uri("http://example.com/a b"),
            new Uri("http://example.com/%41"),
            new Uri("http://example.com/A"),
            new Uri("http://example.com/z"),
            new Uri("a", UriKind.Relative),
            new Uri("A%2Fb", UriKind.Relative),
            new Uri("a/b", UriKind.Relative),
            new Uri("/clip.mp4", UriKind.Relative),
            new Uri("zulu", UriKind.Relative),
        ];

        List<SitemapVideo> videos = pool
            .Select(uri => new SitemapVideo { Title = "t", Description = "d", ContentLocation = uri })
            .ToList();
        videos.Add(new SitemapVideo { Title = "t", Description = "d" });

        List<string> findings = [];
        foreach (SitemapVideo a in videos)
        {
            foreach (SitemapVideo b in videos)
            {
                if (Math.Sign(a.CompareTo(b)) != -Math.Sign(b.CompareTo(a)))
                {
                    findings.Add($"ASYM '{a.ContentLocation}' vs '{b.ContentLocation}': {a.CompareTo(b)} / {b.CompareTo(a)}");
                }

                foreach (SitemapVideo c in videos)
                {
                    if (a.CompareTo(b) <= 0 && b.CompareTo(c) <= 0 && a.CompareTo(c) > 0)
                    {
                        findings.Add($"TRANS '{a.ContentLocation}' <= '{b.ContentLocation}' <= '{c.ContentLocation}' but first > last");
                    }
                }

                if (a.Equals(b) && a.GetHashCode() != b.GetHashCode())
                {
                    findings.Add($"MISS '{a.ContentLocation}' equals '{b.ContentLocation}' but hashes differ");
                }
            }
        }

        List<SitemapVideo> shuffled = [.. videos, .. videos, .. videos, .. videos];
        findings.Add($"List<SitemapVideo>({shuffled.Count}).Sort(): {SortOutcome(() => shuffled.Sort())}");

        Report("uri-ordering", findings);
        findings.Where(f => !f.StartsWith("List<", StringComparison.Ordinal)).ToList()
            .ShouldBeEmpty($"URI ordering defects:\n{string.Join("\n", findings.Take(30))}");
    }

    private static string Render(object? value) => value switch
    {
        null => "null",
        string s => $"\"{s}\"",
        _ => value.ToString() ?? string.Empty,
    };

    /// <summary>Yields one instance per candidate value of the supplied member, all others at default.</summary>
    private static IEnumerable<object> Mutations(Type type, PropertyInfo property)
    {
        if (property.CanWrite && property.SetMethod is { IsPublic: true })
        {
            object? baseline = ComparisonContractHarness.TryCreateDefault(type);
            object? original = baseline is null ? null : SafeGet(property, baseline);

            foreach (object? candidate in ComparisonContractHarness.ValuesFor(property.PropertyType))
            {
                if (candidate is null)
                {
                    continue;
                }

                object? instance = ComparisonContractHarness.TryCreateDefault(type);
                if (instance is null)
                {
                    continue;
                }

                bool applied;
                try
                {
                    property.SetValue(instance, candidate);
                    applied = !Equals(SafeGet(property, instance), original);
                }
                catch (Exception)
                {
                    applied = false;
                }

                if (applied)
                {
                    yield return instance;
                }
            }

            yield break;
        }

        if (!property.CanRead)
        {
            yield break;
        }

        foreach (Type concrete in ComparisonContractHarness.ConcreteFor(ElementTypeOf(property.PropertyType)))
        {
            object? instance = ComparisonContractHarness.TryCreateDefault(type);
            if (instance is null || SafeGet(property, instance) is not IList { IsReadOnly: false } list)
            {
                continue;
            }

            object? element = ComparisonContractHarness.TryCreateDefault(concrete);
            if (element is null)
            {
                continue;
            }

            bool added;
            try
            {
                list.Add(element);
                added = true;
            }
            catch (Exception)
            {
                added = false;
            }

            if (added)
            {
                yield return instance;
                yield break;
            }
        }
    }

    private static object? SafeGet(PropertyInfo property, object instance)
    {
        try
        {
            return property.GetValue(instance);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static Type ElementTypeOf(Type listType)
    {
        Type? closed = listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IList<>)
            ? listType
            : listType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
        return closed?.GetGenericArguments()[0] ?? typeof(object);
    }

    /// <summary>
    /// Confirms the XML-RPC sequence comparisons order a disjoint pair rather than calling both
    /// operands the lesser, and that a reordering is a different array.
    /// </summary>
    /// <remarks>
    ///     The sort results are still recorded rather than asserted. <see cref="List{T}.Sort()"/> is not
    ///     a detector on this runtime — <see cref="ComparisonContractSelfCheckTests.SortDetectsAnInconsistentComparer"/>
    ///     establishes that it does not throw even on a deliberately cyclic comparer — so asserting that
    ///     it throws would be asserting something untrue. The antisymmetry checks below are the verdict.
    /// </remarks>
    [TestMethod]
    public void XmlRpcSequenceComparisonsOrderDisjointOperandsRatherThanCallingBothTheLesser()
    {
        List<string> findings = [];

        XmlRpcArrayValue leftArray = new();
        leftArray.Values.Add(new XmlRpcScalarValue("alpha"));
        XmlRpcArrayValue rightArray = new();
        rightArray.Values.Add(new XmlRpcScalarValue("zulu"));

        findings.Add($"XmlRpcArrayValue: a.CompareTo(b)={leftArray.CompareTo(rightArray)}, b.CompareTo(a)={rightArray.CompareTo(leftArray)}");

        XmlRpcMessage leftMessage = new("m");
        leftMessage.Parameters.Add(new XmlRpcScalarValue("alpha"));
        XmlRpcMessage rightMessage = new("m");
        rightMessage.Parameters.Add(new XmlRpcScalarValue("zulu"));

        findings.Add($"XmlRpcMessage: a.CompareTo(b)={leftMessage.CompareTo(rightMessage)}, b.CompareTo(a)={rightMessage.CompareTo(leftMessage)}");

        XmlRpcStructureValue leftStruct = new();
        leftStruct.Members.Add(new XmlRpcStructureMember("k", new XmlRpcScalarValue("alpha")));
        XmlRpcStructureValue rightStruct = new();
        rightStruct.Members.Add(new XmlRpcStructureMember("k", new XmlRpcScalarValue("zulu")));

        findings.Add($"XmlRpcStructureValue: a.CompareTo(b)={leftStruct.CompareTo(rightStruct)}, b.CompareTo(a)={rightStruct.CompareTo(leftStruct)}");

        // Order-insensitivity against an order-sensitive hash.
        XmlRpcArrayValue forward = new();
        forward.Values.Add(new XmlRpcScalarValue("alpha"));
        forward.Values.Add(new XmlRpcScalarValue("zulu"));
        XmlRpcArrayValue reversed = new();
        reversed.Values.Add(new XmlRpcScalarValue("zulu"));
        reversed.Values.Add(new XmlRpcScalarValue("alpha"));

        findings.Add($"XmlRpcArrayValue order: equals={forward.Equals(reversed)}, hashesEqual={forward.GetHashCode() == reversed.GetHashCode()}");

        // The shipping crash: every element pairwise disjoint and the same length, so every comparison
        // answers "lesser" in both directions. Repeats would let some comparison answer 0 and stop the
        // partition loop, which is why the values are unique rather than cycled.
        foreach (int size in (int[])[8, 20, 50, 200])
        {
            List<XmlRpcArrayValue> arrays = [];
            for (int i = 0; i < size; i++)
            {
                XmlRpcArrayValue array = new();
                array.Values.Add(new XmlRpcScalarValue($"unique-{i}"));
                arrays.Add(array);
            }

            findings.Add($"List<XmlRpcArrayValue>({size}).Sort(): {SortOutcome(() => arrays.Sort())}");
        }

        List<XmlRpcStructureValue> structures = [];
        for (int i = 0; i < 50; i++)
        {
            XmlRpcStructureValue structure = new();
            structure.Members.Add(new XmlRpcStructureMember($"key-{i}", new XmlRpcScalarValue($"unique-{i}")));
            structures.Add(structure);
        }

        findings.Add($"List<XmlRpcStructureValue>(50).Sort(): {SortOutcome(() => structures.Sort())}");

        List<XmlRpcMessage> messages = [];
        for (int i = 0; i < 50; i++)
        {
            XmlRpcMessage message = new("m");
            message.Parameters.Add(new XmlRpcScalarValue($"unique-{i}"));
            messages.Add(message);
        }

        findings.Add($"List<XmlRpcMessage>(50).Sort(): {SortOutcome(() => messages.Sort())}");

        // A fixed-seed randomised sweep: mixed lengths, overlapping and disjoint contents, repeats.
        // Introsort's partition loop needs a particular arrangement to run off the end, so a single
        // hand-built list failing to throw is not evidence that none can.
        Random random = new(20260807);
        int throwCount = 0;
        string? firstThrow = null;
        for (int trial = 0; trial < 2000; trial++)
        {
            List<XmlRpcArrayValue> candidates = [];
            int count = random.Next(4, 40);
            for (int i = 0; i < count; i++)
            {
                XmlRpcArrayValue array = new();
                int length = random.Next(0, 4);
                for (int j = 0; j < length; j++)
                {
                    array.Values.Add(new XmlRpcScalarValue($"v{random.Next(0, 6)}"));
                }

                candidates.Add(array);
            }

            string outcome = SortOutcome(candidates.Sort);
            if (outcome != "no throw")
            {
                throwCount++;
                firstThrow ??= $"count={count}: {outcome}";
            }
        }

        findings.Add($"randomised sweep: 2000 sorts, {throwCount} threw. {firstThrow ?? string.Empty}");
        Report("xmlrpc", findings);

        // Roughly eighty lines above this point — six hand-built sorts and a 2,000-iteration randomised
        // sweep — routed every result through SortOutcome, which catches InvalidOperationException and
        // returns a string. That string was appended to the report and never asserted, so none of it
        // could fail. An IComparer inconsistency surfacing as a throw is the shipping crash this file is
        // named for, so it is the verdict, not a note.
        throwCount.ShouldBe(0, $"a sort threw during the randomised sweep: {firstThrow}");

        // The verdict. Each pair is disjoint and equal-length — the exact shape the membership loop
        // could only answer "-1" to, in both directions.
        Math.Sign(leftArray.CompareTo(rightArray)).ShouldBe(-Math.Sign(rightArray.CompareTo(leftArray)));
        leftArray.CompareTo(rightArray).ShouldNotBe(0);
        Math.Sign(leftMessage.CompareTo(rightMessage)).ShouldBe(-Math.Sign(rightMessage.CompareTo(leftMessage)));
        leftMessage.CompareTo(rightMessage).ShouldNotBe(0);
        Math.Sign(leftStruct.CompareTo(rightStruct)).ShouldBe(-Math.Sign(rightStruct.CompareTo(leftStruct)));
        leftStruct.CompareTo(rightStruct).ShouldNotBe(0);

        // An XML-RPC array is ordered, so a reordering is a different array — and cannot be equal with
        // a different hash code.
        forward.Equals(reversed).ShouldBeFalse();
    }

    /// <summary>Attack A: the <see cref="SitemapVideo"/> widening, at the shapes a generator will not reach.</summary>
    [TestMethod]
    public void SitemapVideoWideningSurvivesCollectionOrderRelativeUrisAndEscaping()
    {
        List<string> findings = [];

        // Collection order.
        SitemapVideo tagsForward = new() { Title = "t", Description = "d" };
        tagsForward.Tags.Add("alpha");
        tagsForward.Tags.Add("zulu");
        SitemapVideo tagsReversed = new() { Title = "t", Description = "d" };
        tagsReversed.Tags.Add("zulu");
        tagsReversed.Tags.Add("alpha");
        findings.Add($"Tags order: compare={tagsForward.CompareTo(tagsReversed)}/{tagsReversed.CompareTo(tagsForward)}, equal={tagsForward.Equals(tagsReversed)}, hashEqual={tagsForward.GetHashCode() == tagsReversed.GetHashCode()}");

        SitemapVideo idsForward = new() { Title = "t", Description = "d" };
        idsForward.Identifiers.Add(new SitemapVideoId { Value = "alpha" });
        idsForward.Identifiers.Add(new SitemapVideoId { Value = "zulu" });
        SitemapVideo idsReversed = new() { Title = "t", Description = "d" };
        idsReversed.Identifiers.Add(new SitemapVideoId { Value = "zulu" });
        idsReversed.Identifiers.Add(new SitemapVideoId { Value = "alpha" });
        findings.Add($"Identifiers order: compare={idsForward.CompareTo(idsReversed)}/{idsReversed.CompareTo(idsForward)}, equal={idsForward.Equals(idsReversed)}, hashEqual={idsForward.GetHashCode() == idsReversed.GetHashCode()}");

        SitemapVideo segForward = new() { Title = "t", Description = "d" };
        segForward.ContentSegments.Add(new SitemapVideoSegment { Location = new Uri("http://example.com/a") });
        segForward.ContentSegments.Add(new SitemapVideoSegment { Location = new Uri("http://example.com/z") });
        SitemapVideo segReversed = new() { Title = "t", Description = "d" };
        segReversed.ContentSegments.Add(new SitemapVideoSegment { Location = new Uri("http://example.com/z") });
        segReversed.ContentSegments.Add(new SitemapVideoSegment { Location = new Uri("http://example.com/a") });
        findings.Add($"ContentSegments order: compare={segForward.CompareTo(segReversed)}/{segReversed.CompareTo(segForward)}, equal={segForward.Equals(segReversed)}, hashEqual={segForward.GetHashCode() == segReversed.GetHashCode()}");

        // Relative against absolute — Load accepts UriKind.RelativeOrAbsolute.
        SitemapVideo relative = new() { Title = "t", Description = "d", ContentLocation = new Uri("/clip.mp4", UriKind.Relative) };
        SitemapVideo absolute = new() { Title = "t", Description = "d", ContentLocation = new Uri("http://example.com/clip.mp4") };
        string relativeOutcome;
        try
        {
            relativeOutcome = $"compare={relative.CompareTo(absolute)}/{absolute.CompareTo(relative)}";
        }
        catch (Exception ex)
        {
            relativeOutcome = $"THREW {ex.GetType().Name}: {ex.Message}";
        }

        findings.Add($"relative vs absolute ContentLocation: {relativeOutcome}");

        SitemapVideo relativeTwin = new() { Title = "t", Description = "d", ContentLocation = new Uri("/clip.mp4", UriKind.Relative) };
        findings.Add($"relative twin: equal={relative.Equals(relativeTwin)}, hashEqual={relative.GetHashCode() == relativeTwin.GetHashCode()}");

        // Percent-encoding that UriFormat.Unescaped collapses but Uri.ToString does not.
        SitemapVideo encoded = new() { Title = "t", Description = "d", ContentLocation = new Uri("http://example.com/a%2Fb") };
        SitemapVideo plain = new() { Title = "t", Description = "d", ContentLocation = new Uri("http://example.com/a/b") };
        findings.Add($"%2F vs /: compare={encoded.CompareTo(plain)}, equal={encoded.Equals(plain)}, hashEqual={encoded.GetHashCode() == plain.GetHashCode()}");
        findings.Add($"   Uri.ToString: '{encoded.ContentLocation}' vs '{plain.ContentLocation}'");

        // Empty collection against a populated one, and a default video against itself.
        SitemapVideo empty = new() { Title = "t", Description = "d" };
        SitemapVideo oneTag = new() { Title = "t", Description = "d" };
        oneTag.Tags.Add("alpha");
        findings.Add($"empty vs one tag: compare={empty.CompareTo(oneTag)}/{oneTag.CompareTo(empty)}");

        // Sorting fifty varied videos.
        List<SitemapVideo> videos = [];
        for (int i = 0; i < 50; i++)
        {
            SitemapVideo video = new()
            {
                Title = $"title-{i % 6}",
                Description = $"desc-{i % 4}",
                ContentLocation = new Uri($"http://example.com/{i % 3}"),
                ViewCount = i % 5,
                Live = i % 2 == 0,
            };
            video.Tags.Add($"tag-{i % 7}");
            video.Identifiers.Add(new SitemapVideoId { Value = $"id-{i % 3}" });
            videos.Add(video);
        }

        string sortOutcome;
        try
        {
            videos.Sort();
            sortOutcome = "no throw";
        }
        catch (InvalidOperationException ex)
        {
            sortOutcome = $"{ex.GetType().Name}: {ex.Message}";
        }

        findings.Add($"List<SitemapVideo>.Sort(): {sortOutcome}");

        Report("sitemapvideo", findings);

        // The verdict, at the shapes a generated population will not reach.
        // Collection order is consulted, in the comparison and the hash alike.
        tagsForward.Equals(tagsReversed).ShouldBeFalse();
        idsForward.Equals(idsReversed).ShouldBeFalse();
        segForward.Equals(segReversed).ShouldBeFalse();

        // A relative location neither throws nor collapses onto an absolute one, and two identical
        // relative locations still agree.
        relative.CompareTo(absolute).ShouldNotBe(0);
        relative.Equals(relativeTwin).ShouldBeTrue();
        relative.GetHashCode().ShouldBe(relativeTwin.GetHashCode());

        // RFC 3986 §2.2: a percent-encoded reserved character is not the literal character.
        encoded.Equals(plain).ShouldBeFalse();

        // An empty collection orders before a populated one, both ways round.
        Math.Sign(empty.CompareTo(oneTag)).ShouldBe(-Math.Sign(oneTag.CompareTo(empty)));
    }

    /// <summary>Attack C: confirm the Yahoo Media hash is coarser than its comparison, not finer.</summary>
    [TestMethod]
    public void YahooMediaHashIsCoarserThanItsComparisonAndNotFiner()
    {
        List<string> findings = [];

        // A member the comparison consults but the hash omits: cost only.
        YahooMediaContent contentLeft = new();
        contentLeft.Keywords.Add("alpha");
        YahooMediaContent contentRight = new();
        contentRight.Keywords.Add("zulu");
        findings.Add($"YahooMediaContent Keywords: compare={contentLeft.CompareTo(contentRight)}, hashEqual={contentLeft.GetHashCode() == contentRight.GetHashCode()}");

        YahooMediaGroup groupLeft = new();
        groupLeft.Keywords.Add("alpha");
        YahooMediaGroup groupRight = new();
        groupRight.Keywords.Add("zulu");
        findings.Add($"YahooMediaGroup Keywords: compare={groupLeft.CompareTo(groupRight)}, hashEqual={groupLeft.GetHashCode() == groupRight.GetHashCode()}");

        // The dangerous direction: equal under CompareTo but different hashes.
        YahooMediaContent nullLanguage = new();
        YahooMediaContent invariantLanguage = new() { Language = System.Globalization.CultureInfo.InvariantCulture };
        findings.Add($"YahooMediaContent null vs invariant Language: equal={nullLanguage.Equals(invariantLanguage)}, hashEqual={nullLanguage.GetHashCode() == invariantLanguage.GetHashCode()}");

        Report("yahoomedia", findings);

        // Coarser is only a collision: Keywords separates the comparison but not the hash, which costs
        // a bucket and never a miss. It is asserted so that the claim in the method name is checked.
        contentLeft.CompareTo(contentRight).ShouldNotBe(0);
        groupLeft.CompareTo(groupRight).ShouldNotBe(0);

        // Finer is a lookup miss, and this is the pair that was one: an absent language and the
        // invariant culture compare equal, so they must hash equally.
        nullLanguage.Equals(invariantLanguage).ShouldBeTrue();
        nullLanguage.GetHashCode().ShouldBe(invariantLanguage.GetHashCode());
    }

    /// <summary>Attack B: <c>Documentation</c> is in every extension's hash and comparison.</summary>
    [TestMethod]
    public void ExtensionDocumentationIsConsistentAcrossTheFamily()
    {
        List<string> findings = [];

        AtomPublishingControlSyndicationExtension control = new();
        AtomPublishingEditedSyndicationExtension edited = new();
        findings.Add($"AtomPublishingControl.Documentation = {control.Documentation}");
        findings.Add($"AtomPublishingEdited.Documentation   = {edited.Documentation}");
        findings.Add($"shared = {Equals(control.Documentation, edited.Documentation)}");

        List<Type> extensionTypes = ComparisonContractHarness.ProductAssemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(SyndicationExtension).IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

        findings.Add($"concrete extension types = {extensionTypes.Count}");
        foreach (Type type in extensionTypes)
        {
            if (Activator.CreateInstance(type) is SyndicationExtension extension)
            {
                findings.Add($"  {type.Name}\t{extension.Documentation}");
            }
        }

        Report("documentation", findings);
        extensionTypes.Count.ShouldBeGreaterThan(20);

        // The property the method name claims, and the attack it is actually mounting. Documentation is
        // folded into every extension's hash and comparison, and these two extensions deliberately cite
        // the same specification — so if Documentation were the discriminator, they would collide.
        control.Documentation.ShouldBe(edited.Documentation, "both halves of Atom Publishing cite RFC 5023");
        control.Equals(edited).ShouldBeFalse("sharing a documentation link must not make two extension types equal");
        control.GetHashCode().ShouldNotBe(edited.GetHashCode());

        // And every shipped extension arrives carrying one, so no member of the family is compared or
        // hashed against a null.
        foreach (Type type in extensionTypes)
        {
            SyndicationExtension extension = (SyndicationExtension)Activator.CreateInstance(type)!;
            extension.Documentation.ShouldNotBeNull($"{type.Name} carries no documentation link");
            extension.Documentation!.IsAbsoluteUri.ShouldBeTrue($"{type.Name} carries a relative documentation link");
        }
    }
}