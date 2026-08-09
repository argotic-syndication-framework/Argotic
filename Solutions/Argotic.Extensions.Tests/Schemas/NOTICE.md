# NOTICE — schemas embedded in the test assembly

These schemas are compiled into `Argotic.Extensions.Tests` as embedded resources and used to validate
what the library writes. They are **not** part of the Argotic source, are **not** covered by Argotic's
Apache-2.0 licence except where stated below, and are never published: the test project sets
`IsPackable=false`, so none of this reaches a NuGet package.

The `.xsd` files below are **byte-for-byte reproductions** of what the publisher served, retrieved
2026-08-07. A checksum against the canonical URL still matches. They are unmodified and each sits in its
own file, so this is aggregation rather than a derivative work — but both carry share-alike terms, so
anyone reusing one should carry this notice with it.

If you are a rights holder and would like a file removed, please open an issue on the Argotic
repository.

## Third-party, redistributed here

### sitemaps.org — CC BY-SA 2.5

<https://www.sitemaps.org/terms.php> licenses "the Sponsors' copyrights in the sitemaps protocol
specification, as published on the Website" under the Creative Commons Attribution-ShareAlike License
version 2.5. The terms draw no distinction between the prose specification and the schema files, and
neither `.xsd` carries a notice of its own.

- `sitemap-0.9.xsd` — Sitemap 0.9 XML Schema
  Canonical: <https://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd>
- `siteindex-0.9.xsd` — Sitemap index 0.9 XML Schema
  Canonical: <https://www.sitemaps.org/schemas/sitemap/0.9/siteindex.xsd>

### APML Workgroup — CC BY-SA 3.0

The APML 0.6 specification states it is "offered by the Faraday Media and the APML Workgroup under the
terms of the Attribution/Share Alike Creative Commons licence".

- `apml-0.6.xsd` — APML 0.6 XML Schema, target namespace `http://www.apml.org/apml-0.6`
  Canonical: <https://github.com/apml/spec-0.6>

## Ours

- `xhtml-link.xsd` — © 2026 endjin limited, Apache-2.0 with the rest of Argotic.
  A single-element declaration written for this suite, **not** a copy of the XHTML schema and not a
  substitute for one. Its own header explains why it exists and what it deliberately does not assert.

## Deliberately absent

Google's three sitemap extension schemas — `sitemap-news-0.9`, `sitemap-image-1.1` and
`sitemap-video-1.1` — are **not** in this directory and must not be added.

Each carries `Copyright 2010 Google Inc. All Rights Reserved.` in its own header. That is an explicit
reservation with no licence grant in the file, and Google's CC BY 4.0 site policy covers documentation
on `developers.google.com` rather than schema files served from `www.google.com/schemas/`. Copying them
into a public Apache-2.0 repository is not clearly permitted.

They are still validated against, and against Google's own file rather than a paraphrase of it: the
integration tier fetches them in-process at test time, validates, and discards them. See
`Scenarios/ValidateAgainstPublishedSchemas.cs`. **Do not "solve" a failing integration run by vendoring
them here, and do not hand-write an equivalent and present it as one** — an approximation that passes
would be worse than the check being absent, because it would look like conformance and not be.