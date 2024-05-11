document.addEventListener("DOMContentLoaded", () => {
    const buildForm = document.getElementById("buildForm");
    const buildResult = document.getElementById("buildResult");
    const parseForm = document.getElementById("parseForm");
    const parseResult = document.getElementById("parseResult");

    const metrics = {
        builds: document.getElementById("totalBuilds"),
        parses: document.getElementById("totalParses"),
        errors: document.getElementById("totalErrors"),
        last: document.getElementById("lastOperation")
    };

    const refreshMetrics = async () => {
        try {
            const res = await fetch("/admin/iso-preview/metrics");
            if (!res.ok) return;
            const data = await res.json();
            if (metrics.builds) metrics.builds.textContent = data.totalBuilds;
            if (metrics.parses) metrics.parses.textContent = data.totalParses;
            if (metrics.errors) metrics.errors.textContent = data.totalErrors;
            if (metrics.last) metrics.last.textContent = data.lastOperation;
        } catch (err) {
            console.error("Metrics update failed", err);
        }
    };

    const showResult = (element, content, isError = false) => {
        element.classList.remove("hidden");
        element.classList.toggle("bg-red-50", isError);
        element.innerHTML = `<pre class="whitespace-pre-wrap break-all overflow-x-auto">${content}</pre>`;
    };


    buildForm?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const formData = new FormData(buildForm);
        const payload = {
            mti: formData.get("mti"),
            fields: JSON.parse(formData.get("fields") || "{}")
        };

        try {
            const res = await fetch("/api/iso/build", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            const text = await res.text();
            showResult(buildResult, text, !res.ok);
        } catch (err) {
            showResult(buildResult, `Error: ${err.message}`, true);
        }

        refreshMetrics();
        setInterval(refreshMetrics, 10000);
    });

    parseForm?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const formData = new FormData(parseForm);
        const payload = {
            payload: formData.get("payload"),
            encoding: formData.get("encoding")
        };

        try {
            const res = await fetch("/api/iso/parse", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            const json = await res.json();
            showResult(parseResult, JSON.stringify(json, null, 2), !res.ok);
        } catch (err) {
            showResult(parseResult, `Error: ${err.message}`, true);
        }

        refreshMetrics();
        setInterval(refreshMetrics, 10000);
    });

    // Initial metrics + periodic refresh
    refreshMetrics();
    setInterval(refreshMetrics, 10000);
});
