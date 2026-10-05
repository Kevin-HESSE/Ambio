// Ambio — MVP screen mockups (see docs/ui.md).
// Every screen is rendered from the same markup at 390 px and 1440 px; the
// layout switches with the container query in ambio.css. Demo data is
// fictional.

(function () {
  const ic = (n, cls = "") => `<svg class="bi ${cls}" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">${window.BI[n] || ""}</svg>`;
  // The Orbit mark (design-system.md#logo), drawn in the logo-mark square.
  const MARK = `<svg viewBox="0 0 32 32" aria-hidden="true"><path d="M10.7 21.3A7.5 7.5 0 1 1 21.3 10.7" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round"/><circle cx="21.3" cy="10.7" r="3" fill="currentColor"/><circle cx="16" cy="16" r="2.2" fill="currentColor"/></svg>`;

  const STATUS = {
    draft: { label: "Draft", icon: "pencil" },
    applied: { label: "Applied", icon: "send" },
    interview: { label: "Interview", icon: "people" },
    offer: { label: "Offer received", icon: "trophy" },
    rejected: { label: "Rejected", icon: "x-circle" },
    withdrawn: { label: "Withdrawn", icon: "slash-circle" },
  };
  const COUNTS = { draft: 2, applied: 9, interview: 3, offer: 1, rejected: 6, withdrawn: 2 };
  const badge = (s) => `<span class="status-badge status-${s}">${ic(STATUS[s].icon)}${STATUS[s].label}</span>`;
  const tag = (label, icon, style = "") => `<span class="tag ${style ? "tag-" + style : ""}">${icon ? ic(icon) : ""}${label}</span>`;
  const CHANNEL_ICON = { "Online form": "globe", Email: "envelope", Referral: "link-45deg", "In person": "geo-alt" };
  const channelTag = (c) => tag(c, CHANNEL_ICON[c]);

  // ---------- Demo data ----------
  const APPS = [
    { id: "482913", title: "Backend Developer", company: "Brightwave", status: "interview", channel: "Online form", sent: "3 Sep 2026", go: "application-overview" },
    { id: "517204", title: "Lead Developer", company: "Talentis Recruitment", status: "draft", channel: "Email", sent: null, go: "application-status" },
    { id: "305871", title: ".NET Developer", company: "Sofinex Consulting", status: "applied", channel: "Email", sent: "15 Sep 2026" },
    { id: "662140", title: "Software Engineer", company: "Lumen Health", status: "applied", channel: "Email", sent: "8 Sep 2026", spontaneous: true },
    { id: "148356", title: "Full-stack Engineer", company: "Kelvio", status: "applied", channel: "Online form", sent: "3 Sep 2026" },
    { id: "920617", title: "C# Developer", company: "Atelier Nord", status: "offer", channel: "Referral", sent: "28 Jul 2026" },
    { id: "731905", title: "Platform Engineer", company: "Orbital Freight", status: "rejected", channel: "Online form", sent: "20 Aug 2026", go: "application-archived" },
    { id: "284479", title: "DevOps Engineer", company: "Kelvio", status: "withdrawn", channel: "Online form", sent: "1 Aug 2026" },
  ];

  const OFFERS = [
    { title: "Lead Developer", company: "Talentis Recruitment", contract: "Permanent", remote: "Full remote", postings: 1, apps: 1, pub: "25 Sep 2026" },
    { title: "Data Engineer", company: "Lumen Health", contract: "Fixed-term", remote: "On-site", postings: 1, apps: 0, pub: "21 Sep 2026" },
    { title: ".NET Developer", company: "Sofinex Consulting", contract: "Permanent", remote: "Hybrid", postings: 2, apps: 1, pub: "10 Sep 2026" },
    { title: "Backend Developer", company: "Brightwave", contract: "Permanent", remote: "Hybrid", postings: 2, apps: 1, pub: "30 Aug 2026" },
    { title: "Full-stack Engineer", company: "Kelvio", contract: "Permanent", remote: "Hybrid", postings: 3, apps: 1, pub: "28 Aug 2026", go: "offer-detail" },
    { title: "Mobile Developer", company: "Brightwave", contract: "Permanent", remote: "Hybrid", postings: 1, apps: 0, pub: "26 Aug 2026" },
    { title: "Platform Engineer", company: "Orbital Freight", contract: "Freelance", remote: "Full remote", postings: 1, apps: 1, pub: "15 Aug 2026" },
    { title: "DevOps Engineer", company: "Kelvio", contract: "Permanent", remote: "Hybrid", postings: 1, apps: 1, pub: "22 Jul 2026" },
    { title: "C# Developer", company: "Atelier Nord", contract: "Permanent", remote: "On-site", postings: 2, apps: 1, pub: "20 Jul 2026" },
  ];

  const COMPANIES = [
    { name: "Brightwave", kind: "Employer", industry: "Software", location: "Lyon", offers: 2, apps: 1, go: "company-detail" },
    { name: "Kelvio", kind: "Employer", industry: "SaaS", location: "Nantes", offers: 2, apps: 2 },
    { name: "Talentis Recruitment", kind: "Recruitment agency", industry: "Recruitment", location: "Paris", offers: 1, apps: 1 },
    { name: "Sofinex Consulting", kind: "IT services", industry: "Consulting", location: "Paris", offers: 1, apps: 1 },
    { name: "Lumen Health", kind: "Employer", industry: "Healthcare", location: "Bordeaux", offers: 1, apps: 1 },
    { name: "Orbital Freight", kind: "Employer", industry: "Logistics", location: "Lille", offers: 1, apps: 1 },
    { name: "Atelier Nord", kind: "Employer", industry: "Retail", location: "Lille", offers: 1, apps: 1 },
  ];

  // ---------- Shell ----------
  const NAV = [
    { id: "dashboard", label: "Dashboard", tab: "Home", icon: "house-door", iconOn: "house-door-fill", go: "dashboard" },
    { id: "applications", label: "Applications", tab: "Applications", icon: "card-checklist", iconOn: "card-checklist", go: "applications", count: 23 },
    { id: "offers", label: "Offers", tab: "Offers", icon: "briefcase", iconOn: "briefcase-fill", go: "offers", count: 9 },
    { id: "companies", label: "Companies", tab: "Companies", icon: "building", iconOn: "building-fill", go: "companies", count: 7 },
  ];

  function sidebar(active) {
    const items = NAV.map((n) => `
      <a class="nav-item ${active === n.id ? "active" : ""}" href="#${n.go}">${ic(active === n.id ? n.iconOn : n.icon)}${n.label}${n.count ? `<span class="count">${n.count}</span>` : ""}</a>`).join("");
    return `
    <aside class="sidebar" aria-label="Main navigation">
      <a class="logo" href="#dashboard"><span class="logo-mark">${MARK}</span>Ambio</a>
      <div class="nav-label t-eyebrow">Tracking</div>
      ${items}
      <div class="sidebar-foot">
        <a class="nav-item ${active === "settings" ? "active" : ""}" href="#settings">${ic("gear")}Settings</a>
        <div class="segmented icons block" role="group" aria-label="Theme">
          <button class="active" title="System">${ic("circle-half")}<span class="sr-only">System</span></button>
          <button title="Light">${ic("sun")}<span class="sr-only">Light</span></button>
          <button title="Dark">${ic("moon-stars")}<span class="sr-only">Dark</span></button>
        </div>
        <div class="account">
          <span class="avatar">JD</span>
          <a class="email" href="#account">jane.doe@example.com</a>
          <button class="btn btn-icon btn-sm" title="Log out">${ic("box-arrow-right")}</button>
        </div>
      </div>
    </aside>`;
  }

  function tabbar(active) {
    const items = NAV.map((n) => `
      <a class="${active === n.id ? "active" : ""}" href="#${n.go}"><span class="ind">${ic(active === n.id ? n.iconOn : n.icon)}</span>${n.tab}</a>`).join("");
    return `<nav class="tabbar" aria-label="Main navigation">${items}
      <a class="${active === "more" ? "active" : ""}" href="#more"><span class="ind">${ic("three-dots")}</span>More</a></nav>`;
  }

  function appbar({ title, back, actions = "" }) {
    const lead = back
      ? `<a class="btn btn-icon" href="#${back}" aria-label="Back">${ic("arrow-left")}</a>`
      : `<span class="logo-mark" aria-hidden="true">${MARK}</span>`;
    return `<header class="appbar">${lead}<div class="appbar-title">${title}</div><div class="appbar-actions">${actions}</div></header>`;
  }

  function pageHeader(title, { sub = "", actions = "" } = {}) {
    return `<div class="page-header"><div><h1 class="page-title">${title}</h1>${sub ? `<p class="secondary">${sub}</p>` : ""}</div><div class="actions">${actions}</div></div>`;
  }

  function shell({ nav, title, back, barActions, body, fab = false, overlay = "", toast = "" }) {
    return `
    <div class="app">
      ${sidebar(nav)}
      <div class="app-main">
        ${appbar({ title, back, actions: barActions })}
        <div class="app-scroll"><main class="page">${body}</main></div>
        ${tabbar(nav)}
      </div>
      ${fab ? `<a class="fab" href="#application-new" aria-label="New application">${ic("plus-lg")}</a>` : ""}
      ${toast}
      ${overlay}
    </div>`;
  }

  const moreBtn = `<button class="btn btn-icon" aria-label="More actions">${ic("three-dots-vertical")}</button>`;
  const newAppBtn = `<a class="btn btn-primary" href="#application-new">${ic("plus-lg")}New application</a>`;

  // ---------- Dashboard ----------
  function dashboard() {
    const bar = Object.keys(STATUS).map((s) => `<span class="status-${s}" style="flex:${COUNTS[s]}" title="${STATUS[s].label}: ${COUNTS[s]}"></span>`).join("");
    const stats = Object.keys(STATUS).map((s) => `
      <a class="stat status-${s}" href="#applications"><span class="label">${ic(STATUS[s].icon)}${STATUS[s].label}</span><span class="value">${COUNTS[s]}</span></a>`).join("");
    const follow = [
      { t: "Full-stack Engineer", c: "Kelvio", d: 24 },
      { t: "Software Engineer", c: "Lumen Health", d: 19, sp: true },
      { t: ".NET Developer", c: "Sofinex Consulting", d: 12 },
    ].map((f) => `
      <li><a class="list-item" href="#application-overview">
        <div class="grow"><div class="title">${f.t}</div><div class="meta">${f.c}${f.sp ? " · Spontaneous" : ""}</div></div>
        <span class="age">${ic("hourglass-split")}${f.d} days</span>${ic("chevron-right")}
      </a></li>`).join("");
    const feed = [
      { s: "draft", html: "<b>Lead Developer</b> · Talentis Recruitment was created as a draft", when: "Yesterday" },
      { s: "rejected", html: "<b>Platform Engineer</b> · Orbital Freight moved to Rejected", when: "22 Sep" },
      { s: "applied", html: "<b>.NET Developer</b> · Sofinex Consulting was sent", when: "15 Sep" },
      { s: "interview", html: "<b>Backend Developer</b> · Brightwave moved to Interview", when: "12 Sep" },
    ].map((a) => `<li class="status-${a.s}"><span class="ico">${ic(STATUS[a.s].icon)}</span><div class="txt">${a.html}<time>${a.when}</time></div></li>`).join("");

    return shell({
      nav: "dashboard", title: "Dashboard", fab: true,
      body: `
      ${pageHeader("Dashboard", { sub: "Sunday 27 September 2026", actions: newAppBtn })}
      <section class="card">
        <div class="card-header"><h2>Pipeline</h2><span class="card-sub num">23 active applications</span></div>
        <div class="card-body" style="display:flex;flex-direction:column;gap:16px">
          <div class="pipeline-bar" role="img" aria-label="Applications by status">${bar}</div>
          <div class="stat-grid">${stats}</div>
        </div>
      </section>
      <div class="dash-cols">
        <section class="card">
          <div class="card-header"><div><h2>Follow up</h2><div class="card-sub">Applied more than 7 days ago, no news since</div></div></div>
          <ul class="list">${follow}</ul>
        </section>
        <section class="card">
          <div class="card-header"><h2>Recent activity</h2></div>
          <ul class="feed">${feed}</ul>
        </section>
      </div>`,
    });
  }

  // ---------- Applications list ----------
  function appCard(a) {
    return `
    <li><a class="app-card" href="#${a.go || "application-overview"}">
      <div class="row1"><span class="title">${a.title}</span><time>${a.sent || "Not sent"}</time></div>
      <div class="company">${a.company}</div>
      <div class="tags">${badge(a.status)}${a.spontaneous ? tag("Spontaneous", "lightning-charge", "accent") : ""}</div>
    </a></li>`;
  }
  function appRow(a) {
    return `
    <tr>
      <td class="cell-title"><a href="#${a.go || "application-overview"}">${a.title}</a><div class="sub">${a.company}</div></td>
      <td>${badge(a.status)}</td>
      <td><div class="tags">${channelTag(a.channel)}${a.spontaneous ? tag("Spontaneous", "lightning-charge", "accent") : ""}</div></td>
      <td class="date">${a.sent || '<span class="muted">Not sent</span>'}</td>
      <td class="actions">${`<button class="btn btn-icon btn-sm" aria-label="Actions">${ic("three-dots")}</button>`}</td>
    </tr>`;
  }

  function applications() {
    const chips = [`<button class="chip active">All <span class="n">23</span></button>`]
      .concat(Object.keys(STATUS).map((s) => `<button class="chip status-${s}">${ic(STATUS[s].icon)}${STATUS[s].label} <span class="n">${COUNTS[s]}</span></button>`)).join("");
    return shell({
      nav: "applications", title: "Applications", fab: true,
      barActions: `<button class="btn btn-icon" aria-label="Sort">${ic("sort-down")}</button>`,
      body: `
      ${pageHeader("Applications", { sub: "23 applications, archived ones hidden", actions: newAppBtn })}
      <div class="filter-bar">
        <label class="input-icon">${ic("search")}<input class="form-control" placeholder="Search by title or company" aria-label="Search"></label>
        <select class="form-select" aria-label="Status"><option>All statuses</option></select>
        <select class="form-select" aria-label="Company"><option>All companies</option></select>
        <select class="form-select" aria-label="Kind"><option>Offer and spontaneous</option></select>
        <label class="check"><input type="checkbox">Show archived</label>
      </div>
      <div class="list-tools">
        <label class="input-icon">${ic("search")}<input class="form-control" placeholder="Search" aria-label="Search"></label>
        <button class="btn btn-secondary">${ic("sliders")}Filters</button>
      </div>
      <div class="chips" role="group" aria-label="Status">${chips}</div>
      <section class="card app-cards-card"><ul class="list">${APPS.map(appCard).join("")}</ul></section>
      <section class="card table-wrap">
        <table class="table">
          <thead><tr><th>Position</th><th>Status</th><th>Channel</th><th>Sent on ${ic("chevron-down")}</th><th><span class="sr-only">Actions</span></th></tr></thead>
          <tbody>${APPS.map(appRow).join("")}</tbody>
        </table>
        <div class="table-foot"><span class="num">Showing 8 of 23</span><button class="btn btn-secondary btn-sm">Load more</button></div>
      </section>
      <p class="result-line only-mobile num">Showing 8 of 23</p>`,
    });
  }

  function applicationsEmpty() {
    return shell({
      nav: "applications", title: "Applications", fab: true,
      body: `
      ${pageHeader("Applications", { actions: newAppBtn })}
      <section class="card">
        <div class="empty">
          <div class="empty-icon">${ic("inbox")}</div>
          <h2 class="t-h3">No applications yet</h2>
          <p>Log the jobs you apply to and follow each one from draft to offer. Start from a job ad, or record a spontaneous application.</p>
          <div class="actions"><a class="btn btn-primary" href="#application-new">${ic("plus-lg")}New application</a><a class="btn btn-secondary" href="#offer-new">Add an offer</a></div>
        </div>
      </section>`,
    });
  }

  // ---------- Application detail ----------
  const DETAILS = {
    brightwave: {
      title: "Backend Developer", company: "Brightwave", place: "Lyon", status: "interview", since: "since 12 Sep",
      tags: [tag("Permanent", "briefcase"), tag("Hybrid", "house-door"), tag("€50–58k")],
      history: [
        { s: "interview", when: "12 Sep 2026", note: "First call with the HR team" },
        { s: "applied", when: "3 Sep 2026", note: "Sent through the Welcome to the Jungle form" },
        { s: "draft", when: "1 Sep 2026", note: "" },
      ],
      dl: [
        ["Offer", `<a href="#offer-detail">Backend Developer · Brightwave</a>`],
        ["Applied via", `<a href="#offer-detail">Welcome to the Jungle ${ic("box-arrow-up-right")}</a>`],
        ["Channel", channelTag("Online form")],
        ["Sent on", `<time>3 Sep 2026</time>`],
        ["Short ID", `<span class="mono">482913</span>`],
        ["Created", `<time>1 Sep 2026</time>`],
      ],
      notes: "Recommended by a former colleague who works on the platform team.\nStack: .NET 10, PostgreSQL, Azure. Ask about on-call duty.",
      counts: { letters: 2, cv: 1 },
    },
    orbital: {
      title: "Platform Engineer", company: "Orbital Freight", place: "Lille", status: "rejected", since: "since 22 Sep", archived: true,
      tags: [tag("Freelance", "briefcase"), tag("Full remote", "house-door"), tag("Archived", "archive", "archived")],
      history: [
        { s: "rejected", when: "22 Sep 2026", note: "Generic rejection email" },
        { s: "applied", when: "20 Aug 2026", note: "" },
        { s: "draft", when: "18 Aug 2026", note: "" },
      ],
      dl: [
        ["Offer", `<a href="#offer-detail">Platform Engineer · Orbital Freight</a>`],
        ["Applied via", `<a href="#offer-detail">Indeed ${ic("box-arrow-up-right")}</a>`],
        ["Channel", channelTag("Online form")],
        ["Sent on", `<time>20 Aug 2026</time>`],
        ["Short ID", `<span class="mono">731905</span>`],
        ["Archived", `<time>27 Sep 2026</time>`],
      ],
      notes: "No feedback given. The team wanted Kubernetes operators experience.",
      counts: { letters: 1, cv: 1 },
    },
    talentis: {
      title: "Lead Developer", company: "Talentis Recruitment", place: "Paris", status: "draft", since: "since 26 Sep",
      tags: [tag("Permanent", "briefcase"), tag("Full remote", "house-door")],
      history: [{ s: "draft", when: "26 Sep 2026", note: "" }],
      dl: [
        ["Offer", `<a href="#offer-detail">Lead Developer · Talentis Recruitment</a>`],
        ["Applied via", `<a href="#offer-detail">LinkedIn ${ic("box-arrow-up-right")}</a>`],
        ["Channel", channelTag("Email")],
        ["Sent on", `<span class="muted">Not sent yet</span>`],
        ["Short ID", `<span class="mono">517204</span>`],
        ["Created", `<time>26 Sep 2026</time>`],
      ],
      notes: "The recruiter asked for a CV in English.",
      counts: { letters: 0, cv: 0 },
    },
  };

  function letters() {
    return `
    <div class="letters">
      <section class="card letter-list">
        <div class="card-header"><h2>Letters</h2><button class="btn btn-ghost btn-sm">${ic("plus-lg")}New</button></div>
        <ul class="list">
          <li><a class="list-item selected" href="#application-letters"><div class="grow"><div class="title">Cover letter · Brightwave</div><div class="meta">Edited 2 Sep</div></div></a></li>
          <li><a class="list-item" href="#application-letters"><div class="grow"><div class="title">Short message for the form</div><div class="meta">Edited 3 Sep</div></div></a></li>
        </ul>
      </section>
      <section class="card editor">
        <div class="card-body" style="display:flex;flex-direction:column;gap:16px">
          <div class="field"><label class="form-label" for="lt">Title</label><input id="lt" class="form-control" value="Cover letter · Brightwave"></div>
          <div class="field"><label class="form-label" for="lc">Text</label><textarea id="lc" class="form-control">Dear Brightwave team,

I am applying for the Backend Developer position posted on Welcome to the Jungle. For the last four years I have built and run .NET services on Azure, most recently an event-driven billing platform handling two million invoices a month.

Your move to a modular monolith caught my attention: I led the same migration at my current company and would enjoy doing it again with a team that cares about developer experience.

I would be happy to tell you more in an interview.

Best regards,
Jane Doe</textarea></div>
          <div class="editor-foot"><span class="form-text num">1,842 characters · Saved 2 Sep</span>
            <div class="tags"><button class="btn btn-secondary">${ic("copy")}Copy</button><button class="btn btn-primary">Save</button></div></div>
        </div>
      </section>
    </div>`;
  }

  function cvTab() {
    return `
    <div class="dropzone">
      ${ic("cloud-arrow-up")}
      <div class="t-strong">Drop the PDF you sent here</div>
      <button class="btn btn-secondary btn-sm">Choose a file</button>
      <div class="form-text">PDF only, 5 MB max</div>
    </div>
    <section class="card">
      <div class="card-header"><h2>Sent CVs</h2></div>
      <ul class="list">
        <li><div class="list-item">
          ${ic("file-earmark-pdf")}
          <div class="grow"><div class="title">CV_Backend_2026-09.pdf</div><div class="meta num">184 KB · Uploaded 3 Sep 2026</div></div>
          <button class="btn btn-icon btn-sm" aria-label="Preview">${ic("eye")}</button>
          <button class="btn btn-icon btn-sm" aria-label="Download">${ic("download")}</button>
        </div></li>
      </ul>
    </section>`;
  }

  function overviewTab(d) {
    return `
    <section class="card"><div class="card-body"><dl class="dl">${d.dl.map(([k, v]) => `<div><dt>${k}</dt><dd>${v}</dd></div>`).join("")}</dl></div></section>
    <section class="card">
      <div class="card-header"><h2>Notes</h2><button class="btn btn-ghost btn-sm">${ic("pencil")}Edit</button></div>
      <div class="card-body"><p class="notes">${d.notes}</p></div>
    </section>`;
  }

  function applicationDetail(key, tab, { overlay = "", toast = "", banner = "" } = {}) {
    const d = DETAILS[key];
    const tabs = [
      ["overview", "Overview", null, "application-overview"],
      ["letters", "Cover letters", d.counts.letters, "application-letters"],
      ["cv", "CV", d.counts.cv, "application-cv"],
    ].map(([id, label, n, go]) => `<a class="tab ${tab === id ? "active" : ""}" href="#${go}">${label}${n != null ? `<span class="n">${n}</span>` : ""}</a>`).join("");
    const content = tab === "letters" ? letters() : tab === "cv" ? cvTab() : overviewTab(d);
    const timeline = d.history.map((h) => `<li class="status-${h.s}">${`<div>${badge(h.s)}</div>`}<time class="when">${h.when}</time>${h.note ? `<p class="note">${h.note}</p>` : ""}</li>`).join("");
    const changeBtn = d.archived
      ? `<button class="btn btn-secondary btn-block">${ic("arrow-counterclockwise")}Restore to change status</button>`
      : `<a class="btn btn-primary btn-block" href="#application-status">Change status</a>`;
    return shell({
      nav: "applications", title: "Application", back: "applications", barActions: moreBtn, overlay, toast,
      body: `
      ${banner}
      <div class="detail">
        <div class="detail-head">
          <div class="head-text">
            <nav class="breadcrumb"><a href="#applications">Applications</a>${ic("chevron-right")}<span>${d.title}</span></nav>
            <h1 class="page-title">${d.title}</h1>
            <div class="company"><a href="#company-detail">${d.company}</a> · ${d.place}</div>
            <div class="tags">${d.tags.join("")}</div>
          </div>
          <div class="head-actions"><button class="btn btn-secondary">${ic("pencil")}Edit</button><button class="btn btn-secondary btn-icon" aria-label="More actions">${ic("three-dots")}</button></div>
        </div>
        <section class="card status-panel"><div class="card-body">
          <div class="current"><div style="display:flex;flex-direction:column;gap:6px"><span class="t-eyebrow">Status</span>${badge(d.status)}</div><span class="t-small muted">${d.since}</span></div>
          ${changeBtn}
        </div></section>
        <div class="detail-main">
          <nav class="tabs" aria-label="Application sections">${tabs}</nav>
          ${content}
        </div>
        <section class="card history-panel ${tab === "overview" ? "" : "only-desktop"}">
          <div class="card-header"><h2>History</h2></div>
          <div class="card-body"><ol class="timeline">${timeline}</ol></div>
        </section>
      </div>`,
    });
  }

  function statusDialog() {
    return `
    <div class="overlay">
      <div class="dialog" role="dialog" aria-modal="true" aria-labelledby="cs-title">
        <div class="dialog-head"><h2 id="cs-title">Change status</h2><a class="btn btn-icon" href="#application-overview" aria-label="Close">${ic("x-lg")}</a></div>
        <div class="dialog-body">
          <div class="kv-row">Current status ${badge("draft")}</div>
          <div class="field">
            <span class="form-label">New status</span>
            <div class="radio-list" role="radiogroup">
              <label class="radio-card checked"><input type="radio" name="st" checked>${badge("applied")}<span class="hint">Sent to the employer</span></label>
              <label class="radio-card"><input type="radio" name="st">${badge("withdrawn")}<span class="hint">You dropped it</span></label>
            </div>
            <span class="form-text">From Draft, an application can only be sent or dropped.</span>
          </div>
          <div class="field">
            <label class="form-label" for="cs-date">Sent on</label>
            <label class="input-icon">${ic("calendar3")}<input id="cs-date" class="form-control num" value="27/09/2026"></label>
          </div>
          <div class="field">
            <label class="form-label" for="cs-com">Comment <span class="opt">(optional)</span></label>
            <textarea id="cs-com" class="form-control" style="min-height:80px">Sent by email to the recruiter, CV in English attached.</textarea>
          </div>
        </div>
        <div class="dialog-foot"><a class="btn btn-secondary" href="#application-overview">Cancel</a><button class="btn btn-primary">Change status</button></div>
      </div>
    </div>`;
  }

  // ---------- New application ----------
  function applicationNew() {
    return shell({
      nav: "applications", title: "New application", back: "applications",
      body: `
      ${pageHeader("New application")}
      <section class="card form-page"><div class="card-body" style="display:flex;flex-direction:column;gap:24px">
        <div class="segmented block" role="group" aria-label="Kind">
          <button class="active">${ic("briefcase")}From an offer</button>
          <button>${ic("lightning-charge")}Spontaneous</button>
        </div>
        <div class="form-section">
          <h2>Offer</h2>
          <div class="field">
            <label class="form-label" for="na-offer">Job offer</label>
            <label class="input-icon">${ic("search")}<input id="na-offer" class="form-control focus" value="data engineer"></label>
            <div class="card" style="box-shadow:var(--ambio-shadow-md)">
              <ul class="list">
                <li><a class="list-item" style="background:var(--ambio-bg-subtle)" href="#application-new"><div class="grow"><div class="title">Data Engineer</div><div class="meta">Lumen Health · Bordeaux · Fixed-term · No application yet</div></div>${ic("check2")}</a></li>
                <li><a class="list-item" href="#offer-new">${ic("plus-lg")}<div class="grow"><div class="title" style="color:var(--ambio-accent-text)">Create a new offer “data engineer”</div><div class="meta">Company, title and link, the rest can wait</div></div></a></li>
              </ul>
            </div>
          </div>
        </div>
        <div class="form-section">
          <h2>Application</h2>
          <div class="form">
            <div class="field"><label class="form-label" for="na-via">Applied via</label><select id="na-via" class="form-select"><option>Lumen Health careers page · seen 21 Sep</option></select></div>
            <div class="field"><label class="form-label" for="na-ch">Channel</label><select id="na-ch" class="form-select"><option>Online form</option></select></div>
            <div class="field span-2"><label class="form-label" for="na-cd">Channel detail <span class="opt">(optional)</span></label><input id="na-cd" class="form-control" placeholder="An address like jobs@…, or who referred you"></div>
            <div class="field"><span class="form-label">Status</span><label class="check"><input type="checkbox" checked>Already sent</label></div>
            <div class="field"><label class="form-label" for="na-date">Sent on</label><label class="input-icon">${ic("calendar3")}<input id="na-date" class="form-control num" value="27/09/2026"></label></div>
            <div class="field span-2"><label class="form-label" for="na-notes">Notes <span class="opt">(optional)</span></label><textarea id="na-notes" class="form-control" style="min-height:96px"></textarea></div>
          </div>
        </div>
        <div class="action-bar"><a class="btn btn-secondary" href="#applications">Cancel</a><button class="btn btn-primary">Create application</button></div>
      </div></section>`,
    });
  }

  // ---------- Offers ----------
  function offers() {
    const cards = OFFERS.map((o) => `
      <li><a class="app-card" href="#${o.go || "offer-detail"}">
        <div class="row1"><span class="title">${o.title}</span><time>${o.pub.replace(" 2026", "")}</time></div>
        <div class="company">${o.company}</div>
        <div class="tags">${tag(o.contract, "briefcase")}${tag(o.remote, "house-door")}<span class="t-small muted num">${o.postings} posting${o.postings > 1 ? "s" : ""} · ${o.apps ? o.apps + " application" : "No application"}</span></div>
      </a></li>`).join("");
    const rows = OFFERS.map((o) => `
      <tr>
        <td class="cell-title"><a href="#${o.go || "offer-detail"}">${o.title}</a><div class="sub">${o.company}</div></td>
        <td><div class="tags">${tag(o.contract, "briefcase")}${tag(o.remote, "house-door")}</div></td>
        <td class="num">${o.postings}</td>
        <td class="num">${o.apps ? o.apps : '<a class="btn btn-secondary btn-sm" href="#application-new">Apply</a>'}</td>
        <td class="date">${o.pub}</td>
        <td class="actions"><button class="btn btn-icon btn-sm" aria-label="Actions">${ic("three-dots")}</button></td>
      </tr>`).join("");
    return shell({
      nav: "offers", title: "Offers",
      barActions: `<a class="btn btn-icon" href="#offer-new" aria-label="New offer">${ic("plus-lg")}</a>`,
      body: `
      ${pageHeader("Offers", { sub: "Job ads you saved, applied to or not", actions: `<a class="btn btn-primary" href="#offer-new">${ic("plus-lg")}New offer</a>` })}
      <div class="filter-bar">
        <label class="input-icon">${ic("search")}<input class="form-control" placeholder="Search by title or company" aria-label="Search"></label>
        <select class="form-select" aria-label="Company"><option>All companies</option></select>
        <select class="form-select" aria-label="Contract"><option>All contracts</option></select>
        <label class="check"><input type="checkbox">Without application</label>
        <label class="check"><input type="checkbox">Show archived</label>
      </div>
      <div class="list-tools">
        <label class="input-icon">${ic("search")}<input class="form-control" placeholder="Search" aria-label="Search"></label>
        <button class="btn btn-secondary">${ic("sliders")}Filters</button>
      </div>
      <section class="card app-cards-card"><ul class="list">${cards}</ul></section>
      <section class="card table-wrap">
        <table class="table">
          <thead><tr><th>Offer</th><th>Contract</th><th class="num">Postings</th><th class="num">Applications</th><th>Published ${ic("chevron-down")}</th><th><span class="sr-only">Actions</span></th></tr></thead>
          <tbody>${rows}</tbody>
        </table>
      </section>`,
    });
  }

  function offerDetail() {
    const postings = [
      ["Welcome to the Jungle", "welcometothejungle.com/fr/companies/kelvio/jobs/full-stack-engineer", "28 Aug"],
      ["LinkedIn", "linkedin.com/jobs/view/4102938475", "30 Aug"],
      ["Kelvio careers page", "kelvio.io/careers/full-stack-engineer", "2 Sep"],
    ].map(([s, u, d]) => `
      <li><div class="list-item">${ic("globe")}
        <div class="grow"><div class="title">${s}</div><div class="meta" style="overflow:hidden;text-overflow:ellipsis;white-space:nowrap">${u}</div></div>
        <time class="t-small muted">${d}</time>
        <a class="btn btn-icon btn-sm" href="#offer-detail" aria-label="Open ${s}">${ic("box-arrow-up-right")}</a>
      </div></li>`).join("");
    return shell({
      nav: "offers", title: "Offer", back: "offers", barActions: moreBtn,
      body: `
      <div class="detail">
        <div class="detail-head">
          <div class="head-text">
            <nav class="breadcrumb"><a href="#offers">Offers</a>${ic("chevron-right")}<span>Full-stack Engineer</span></nav>
            <h1 class="page-title">Full-stack Engineer</h1>
            <div class="company"><a href="#company-detail">Kelvio</a> · Nantes · Published 28 Aug 2026</div>
            <div class="tags">${tag("Permanent", "briefcase")}${tag("Hybrid", "house-door")}${tag("€48–55k")}</div>
          </div>
          <div class="head-actions"><button class="btn btn-secondary">${ic("pencil")}Edit</button><button class="btn btn-secondary btn-icon" aria-label="More actions">${ic("three-dots")}</button></div>
        </div>
        <section class="card" style="grid-area:status">
          <div class="card-header"><h2>Postings <span class="card-sub num">3</span></h2><button class="btn btn-ghost btn-sm">${ic("plus-lg")}Add</button></div>
          <ul class="list">${postings}</ul>
        </section>
        <div class="detail-main">
          <section class="card">
            <div class="card-header"><h2>Job ad</h2><span class="card-sub">Copied 28 Aug</span></div>
            <div class="card-body"><p class="prose">Kelvio builds scheduling software for 1,200 physiotherapy practices across France. We are looking for a full-stack engineer to join the booking team (5 people).

What you will do
- Build features across the ASP.NET Core API and the Vue front end
- Own the patient booking flow, from design review to production
- Take part in code reviews and a light on-call rotation

What we are looking for
- 3+ years with C# and a modern JavaScript framework
- Comfortable with SQL and query performance
- French or English

Hybrid: 2 days a week at the Nantes office.</p></div>
          </section>
        </div>
        <section class="card" style="grid-area:history">
          <div class="card-header"><h2>Applications <span class="card-sub num">1</span></h2><a class="btn btn-ghost btn-sm" href="#application-new">${ic("plus-lg")}New</a></div>
          <ul class="list"><li><a class="list-item" href="#application-overview"><div class="grow" style="display:flex;flex-direction:column;gap:6px">${`<div>${badge("applied")}</div>`}<div class="meta">Sent 3 Sep via Welcome to the Jungle</div></div>${ic("chevron-right")}</a></li></ul>
        </section>
      </div>`,
    });
  }

  function offerNew() {
    return shell({
      nav: "offers", title: "New offer", back: "offers",
      body: `
      ${pageHeader("New offer")}
      <div class="banner banner-info form-page">
        ${ic("info-circle")}
        <div class="grow">
          <div><div class="t-strong">This offer may already exist</div><p class="t-small secondary"><a href="#offer-detail">Full-stack Engineer · Kelvio</a>, published 28 Aug with 3 postings. If it's the same job, add this link to it instead of creating a duplicate.</p></div>
          <div class="actions"><a class="btn btn-secondary btn-sm" href="#offer-detail">Add a posting to this offer</a><button class="btn btn-ghost btn-sm">Create a new offer anyway</button></div>
        </div>
      </div>
      <section class="card form-page"><div class="card-body" style="display:flex;flex-direction:column;gap:24px">
        <div class="form-section"><h2>Offer</h2><div class="form">
          <div class="field"><label class="form-label" for="no-co">Company</label><select id="no-co" class="form-select"><option>Kelvio</option></select><span class="form-text">Or type a new name to create the company.</span></div>
          <div class="field"><label class="form-label" for="no-ti">Title</label><input id="no-ti" class="form-control" value="Full stack engineer"></div>
          <div class="field"><label class="form-label" for="no-ct">Contract</label><select id="no-ct" class="form-select"><option>Permanent</option></select></div>
          <div class="field"><label class="form-label" for="no-rp">Remote policy</label><select id="no-rp" class="form-select"><option>Hybrid</option></select></div>
          <div class="field"><label class="form-label" for="no-lo">Location <span class="opt">(optional)</span></label><input id="no-lo" class="form-control" value="Nantes"></div>
          <div class="field"><label class="form-label" for="no-sa">Salary <span class="opt">(optional)</span></label><input id="no-sa" class="form-control" placeholder="As announced, e.g. 45–55 k€"></div>
        </div></div>
        <div class="form-section"><h2>Where you saw it</h2><div class="form">
          <div class="field"><label class="form-label" for="no-si">Site</label><select id="no-si" class="form-select"><option>LinkedIn</option></select></div>
          <div class="field"><label class="form-label" for="no-se">Seen on</label><label class="input-icon">${ic("calendar3")}<input id="no-se" class="form-control num" value="27/09/2026"></label></div>
          <div class="field span-2"><label class="form-label" for="no-url">Link</label><input id="no-url" class="form-control is-invalid" value="linkedin.com/jobs/view/4102938475"><span class="invalid-feedback">${ic("exclamation-triangle")}Enter the full link, starting with https://</span></div>
        </div></div>
        <div class="form-section"><h2>Job ad</h2><div class="form">
          <div class="field span-2"><label class="form-label" for="no-ad">Text of the ad <span class="opt">(optional)</span></label><textarea id="no-ad" class="form-control" placeholder="Paste the ad here: the copy stays when the ad goes offline."></textarea></div>
        </div></div>
        <div class="action-bar"><a class="btn btn-secondary" href="#offers">Cancel</a><button class="btn btn-primary">Create offer</button></div>
      </div></section>`,
    });
  }

  // ---------- Companies ----------
  function companies() {
    const cards = COMPANIES.map((c) => `
      <li><a class="app-card" href="#${c.go || "company-detail"}">
        <div class="row1"><span class="title">${c.name}</span></div>
        <div class="company">${c.industry} · ${c.location}</div>
        <div class="tags">${tag(c.kind, "building")}<span class="t-small muted num">${c.offers} offer${c.offers > 1 ? "s" : ""} · ${c.apps} application${c.apps > 1 ? "s" : ""}</span></div>
      </a></li>`).join("");
    const rows = COMPANIES.map((c) => `
      <tr>
        <td class="cell-title"><a href="#${c.go || "company-detail"}">${c.name}</a><div class="sub">${c.industry}</div></td>
        <td>${tag(c.kind, "building")}</td>
        <td>${c.location}</td>
        <td class="num">${c.offers}</td>
        <td class="num">${c.apps}</td>
        <td class="actions"><button class="btn btn-icon btn-sm" aria-label="Actions">${ic("three-dots")}</button></td>
      </tr>`).join("");
    return shell({
      nav: "companies", title: "Companies",
      barActions: `<button class="btn btn-icon" aria-label="New company">${ic("plus-lg")}</button>`,
      body: `
      ${pageHeader("Companies", { sub: "Employers, agencies and IT services companies", actions: `<button class="btn btn-primary">${ic("plus-lg")}New company</button>` })}
      <div class="filter-bar">
        <label class="input-icon">${ic("search")}<input class="form-control" placeholder="Search by name or industry" aria-label="Search"></label>
        <select class="form-select" aria-label="Kind"><option>All kinds</option></select>
        <label class="check"><input type="checkbox">Show archived</label>
      </div>
      <div class="list-tools">
        <label class="input-icon">${ic("search")}<input class="form-control" placeholder="Search" aria-label="Search"></label>
        <button class="btn btn-secondary">${ic("sliders")}Filters</button>
      </div>
      <section class="card app-cards-card"><ul class="list">${cards}</ul></section>
      <section class="card table-wrap">
        <table class="table">
          <thead><tr><th>Company ${ic("chevron-down")}</th><th>Kind</th><th>Location</th><th class="num">Offers</th><th class="num">Applications</th><th><span class="sr-only">Actions</span></th></tr></thead>
          <tbody>${rows}</tbody>
        </table>
      </section>`,
    });
  }

  function companyDetail() {
    const profile = [
      ["Website", `<a href="#company-detail">brightwave.io ${ic("box-arrow-up-right")}</a>`],
      ["Industry", "Software"],
      ["Size", `<span class="num">51–250 employees</span>`],
      ["Location", "Lyon"],
      ["LinkedIn", `<a href="#company-detail">linkedin.com/company/brightwave ${ic("box-arrow-up-right")}</a>`],
    ].map(([k, v]) => `<div><dt>${k}</dt><dd>${v}</dd></div>`).join("");
    return shell({
      nav: "companies", title: "Company", back: "companies", barActions: moreBtn,
      body: `
      <div class="detail">
        <div class="detail-head">
          <div class="head-text">
            <nav class="breadcrumb"><a href="#companies">Companies</a>${ic("chevron-right")}<span>Brightwave</span></nav>
            <h1 class="page-title">Brightwave</h1>
            <div class="tags">${tag("Employer", "building")}</div>
          </div>
          <div class="head-actions"><button class="btn btn-secondary">${ic("pencil")}Edit</button><button class="btn btn-secondary btn-icon" aria-label="More actions">${ic("three-dots")}</button></div>
        </div>
        <section class="card" style="grid-area:status">
          <div class="card-header"><h2>Profile</h2></div>
          <div class="card-body"><dl class="dl" style="grid-template-columns:1fr">${profile}</dl></div>
        </section>
        <div class="detail-main">
          <section class="card"><div class="card-body"><p>Brightwave makes energy-monitoring software for industrial sites. Series B in 2025, about 120 people, engineering team of 35 split into product squads.</p></div></section>
          <nav class="tabs" aria-label="Company sections"><a class="tab active" href="#company-detail">Offers<span class="n">2</span></a><a class="tab" href="#company-detail">Applications<span class="n">1</span></a></nav>
          <section class="card">
            <ul class="list">
              <li><a class="list-item" href="#offer-detail"><div class="grow" style="display:flex;flex-direction:column;gap:6px"><div class="title">Backend Developer</div><div class="tags">${tag("Permanent", "briefcase")}${tag("Hybrid", "house-door")}${badge("interview")}</div></div>${ic("chevron-right")}</a></li>
              <li><a class="list-item" href="#offer-detail"><div class="grow" style="display:flex;flex-direction:column;gap:6px"><div class="title">Mobile Developer</div><div class="tags">${tag("Permanent", "briefcase")}${tag("Hybrid", "house-door")}<span class="t-small muted">No application</span></div></div>${ic("chevron-right")}</a></li>
            </ul>
          </section>
        </div>
      </div>`,
    });
  }

  // ---------- Navigation & settings ----------
  function moreSheet() {
    const overlay = `
    <div class="overlay">
      <div class="dialog" role="dialog" aria-modal="true" aria-label="More">
        <ul class="menu-list" style="padding:8px 0">
          <li><a href="#settings">${ic("gear")}Settings</a></li>
          <li><a href="#account">${ic("person-circle")}Account</a></li>
        </ul>
        <div class="dialog-body" style="padding-top:8px;border-top:1px solid var(--ambio-border-default)">
          <div class="field" style="padding-top:12px"><span class="form-label">Theme</span>
            <div class="segmented block"><button class="active">${ic("circle-half")}System</button><button>${ic("sun")}Light</button><button>${ic("moon-stars")}Dark</button></div>
          </div>
        </div>
        <ul class="menu-list" style="padding:0 0 12px;border-top:1px solid var(--ambio-border-default)">
          <li><button class="danger">${ic("box-arrow-right")}Log out</button></li>
        </ul>
      </div>
    </div>`;
    const html = dashboard();
    return html.replace(/<nav class="tabbar"[\s\S]*?<\/nav>/, tabbar("more")).replace(/<\/div>\s*$/, overlay + "</div>");
  }

  function settings() {
    return shell({
      nav: "settings", title: "Settings", back: "more",
      body: `
      ${pageHeader("Settings")}
      <section class="card form-page"><div class="card-body" style="display:flex;flex-direction:column;gap:24px">
        <div class="form-section"><h2>Follow-ups</h2>
          <div class="field" style="max-width:320px">
            <label class="form-label" for="fu">Suggest a follow-up after</label>
            <div style="display:flex;gap:8px;align-items:center"><input id="fu" class="form-control num" value="7" style="width:88px">days</div>
            <span class="form-text">Applications still Applied after this delay show up in Follow up on the dashboard.</span>
          </div>
        </div>
        <div class="form-section"><h2>Appearance</h2>
          <div class="field"><span class="form-label">Theme</span>
            <div class="segmented"><button class="active">${ic("circle-half")}System</button><button>${ic("sun")}Light</button><button>${ic("moon-stars")}Dark</button></div>
            <span class="form-text">System follows your device. Saved in this browser.</span>
          </div>
        </div>
        <div class="action-bar"><button class="btn btn-primary">Save settings</button></div>
      </div></section>`,
    }).replace(/<nav class="tabbar"[\s\S]*?<\/nav>/, tabbar("more"));
  }

  function account() {
    const tabs = ["Profile", "Email", "Password", "Two-factor authentication", "Personal data"]
      .map((t, i) => `<a class="tab ${i === 0 ? "active" : ""}" href="#account">${t}</a>`).join("");
    return shell({
      nav: "account", title: "Account", back: "more",
      body: `
      ${pageHeader("Account", { sub: "Manage how you sign in to Ambio" })}
      <div class="manage">
        <nav class="manage-nav" aria-label="Account sections">${tabs}</nav>
        <div class="grow">
          <section class="card"><div class="card-header"><h2>Profile</h2></div><div class="card-body"><div class="form" style="grid-template-columns:1fr">
            <div class="field"><label class="form-label" for="ac-u">Username</label><input id="ac-u" class="form-control" value="jane.doe@example.com" disabled style="background:var(--ambio-bg-subtle);color:var(--ambio-text-secondary)"></div>
            <div class="field"><label class="form-label" for="ac-p">Phone number <span class="opt">(optional)</span></label><input id="ac-p" class="form-control" placeholder="+33 6 12 34 56 78"></div>
            <div class="action-bar"><button class="btn btn-primary">Save</button></div>
          </div></div></section>
        </div>
      </div>`,
    }).replace(/<nav class="tabbar"[\s\S]*?<\/nav>/, tabbar("more"));
  }

  function login() {
    return `
    <div class="app-scroll" style="height:100%"><div class="auth">
      <span class="logo logo-lg"><span class="logo-mark">${MARK}</span>Ambio</span>
      <section class="card"><div class="card-body">
        <div><h1 class="t-h2">Log in</h1><p class="t-small secondary">Use the account created when Ambio was set up.</p></div>
        <div class="field"><label class="form-label" for="li-e">Email</label><input id="li-e" class="form-control" value="jane.doe@example.com" autocomplete="username"></div>
        <div class="field"><div style="display:flex;justify-content:space-between"><label class="form-label" for="li-p">Password</label><a class="t-small" href="#login">Forgot your password?</a></div><input id="li-p" type="password" class="form-control" value="password123" autocomplete="current-password"></div>
        <label class="check"><input type="checkbox" checked>Remember me</label>
        <a class="btn btn-primary btn-block" href="#dashboard">Log in</a>
      </div></section>
      <p class="auth-foot">Ambio has a single account. Registration is closed.</p>
    </div></div>`;
  }

  // ---------- Registry ----------
  const toast = `<div class="toast" role="status">${ic("archive")}<span class="grow">Application archived</span><button>Undo</button></div>`;
  const archivedBanner = `
    <div class="banner">${ic("archive")}
      <div class="grow"><div><div class="t-strong">This application is archived</div><p class="t-small secondary">It's hidden from lists and the dashboard. Restore it to track it again.</p></div>
      <div class="actions"><button class="btn btn-secondary btn-sm">${ic("arrow-counterclockwise")}Restore</button></div></div>
    </div>`;

  window.SCREENS = [
    { id: "login", group: "Log in", title: "Log in", route: "/Account/Login", render: login,
      content: "Centered card with the logo, email, password and “Remember me”. No registration link: the app has a single account (ADR 0002).",
      later: "Phase 5 adds “Continue with GitHub” for a linked account." },
    { id: "dashboard", group: "Dashboard", title: "Dashboard", route: "/", render: dashboard,
      content: "Pipeline bar and six counters by status (each opens the filtered list). Follow up lists Applied applications with no change for the configured delay. Recent activity shows the last status changes.",
      later: "Phase 2b adds an “Upcoming interviews” card and a “Log a follow-up” action that clears an item from Follow up." },
    { id: "applications", group: "Applications", title: "Applications — list", route: "/applications", render: applications,
      content: "Table on desktop, cards on mobile. Desktop filters inline; mobile has search, status chips and a Filters sheet. Archived rows are hidden unless “Show archived” is on.",
      later: "—" },
    { id: "applications-empty", group: "Applications", title: "Applications — empty", route: "/applications", render: applicationsEmpty,
      content: "First run: explains what to do and offers the two ways in, a new application or an offer.",
      later: "—" },
    { id: "application-overview", group: "Applications", title: "Application — Overview", route: "/applications/482913-backend-developer-brightwave", render: () => applicationDetail("brightwave", "overview"),
      content: "Header with title, company and tags. Status panel with “Change status” and the history: right column on desktop, above the tabs (status) and after the content (history) on mobile.",
      later: "Phase 2b adds Interactions and Contacts tabs." },
    { id: "application-letters", group: "Applications", title: "Application — Cover letters", route: "…?tab=letters", render: () => applicationDetail("brightwave", "letters"),
      content: "List of the application's letters and a plain-text editor with Copy and Save. On mobile the list stacks above the editor.",
      later: "—" },
    { id: "application-cv", group: "Applications", title: "Application — CV", route: "…?tab=cv", render: () => applicationDetail("brightwave", "cv"),
      content: "Upload of the PDF actually sent (drop zone or file picker, PDF only, size limit) and the list of sent CVs with preview and download.",
      later: "Phase 3 adds “Generate a CV” from the profile next to the upload." },
    { id: "application-archived", group: "Applications", title: "Application — archived", route: "/applications/731905-platform-engineer-orbital-freight", render: () => applicationDetail("orbital", "overview", { toast, banner: archivedBanner }),
      content: "Right after “Archive” in the ⋯ menu: no confirmation, a toast with Undo, and a banner with Restore on the page. The status can't change until the application is restored.",
      later: "—" },
    { id: "application-new", group: "Applications", title: "New application", route: "/applications/new", render: applicationNew,
      content: "Single page. The toggle picks an offer application or a spontaneous one (company + target position). The offer search suggests existing offers and creates a new one inline. “Already sent” sets the status to Applied with its date; otherwise it's a Draft.",
      later: "—" },
    { id: "application-status", group: "Applications", title: "Change status", route: "modal / bottom sheet", render: () => applicationDetail("talentis", "overview", { overlay: statusDialog() }),
      content: "Offers only the transitions the workflow allows. Draft → Applied asks for the sending date. Optional comment, stored in the history. Bottom sheet on mobile, modal on desktop.",
      later: "—" },
    { id: "offers", group: "Offers", title: "Offers — list", route: "/offers", render: offers,
      content: "Every saved job ad, applied to or not (market watch). Postings and applications counts; “Apply” when there is no application yet.",
      later: "Phase 2b shows the recruiter (agency) next to the end client." },
    { id: "offer-detail", group: "Offers", title: "Offer — detail", route: "/offers/{id}", render: offerDetail,
      content: "Postings (site, link, seen on) and applications in the side column, plain-text copy of the ad in the main column.",
      later: "—" },
    { id: "offer-new", group: "Offers", title: "New offer", route: "/offers/new", render: offerNew,
      content: "Offer, where it was seen (first posting) and the ad text. When the company and a similar title match an existing offer, a banner suggests adding a posting to it instead. Field errors show under the field.",
      later: "Phase 2b adds the recruiter company field." },
    { id: "companies", group: "Companies", title: "Companies — list", route: "/companies", render: companies,
      content: "Name, kind, industry, location, offers and applications counts.",
      later: "—" },
    { id: "company-detail", group: "Companies", title: "Company — detail", route: "/companies/{id}", render: companyDetail,
      content: "Profile in the side column, description and tabs (Offers, Applications) in the main column.",
      later: "Phase 2b adds Notes (research timeline) and Contacts tabs." },
    { id: "more", group: "Navigation & settings", title: "“More” sheet", route: "mobile only", render: moreSheet, mobileOnly: true,
      content: "Fifth tab of the mobile bar. Holds what the desktop sidebar shows at its bottom: Settings, Account, theme, Log out.",
      later: "Phase 3 adds Profile and My CVs, Phase 4 Templates, Phase 6 Export. Entries are added here and in the sidebar, never as new tabs." },
    { id: "settings", group: "Navigation & settings", title: "Settings", route: "/settings", render: settings,
      content: "Follow-up delay (7 days by default, stored in UserSettings) and theme (System, Light, Dark, stored in the browser).",
      later: "Phase 5 adds the language." },
    { id: "account", group: "Navigation & settings", title: "Account", route: "/Account/Manage", render: account,
      content: "Identity management pages in the app shell: section list on the left on desktop, scrolling tabs on mobile. Every other Identity page uses the same template.",
      later: "Phase 5 adds external logins (GitHub)." },
  ];
})();
