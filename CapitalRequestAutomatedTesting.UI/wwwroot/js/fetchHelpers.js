export async function fetchJsonOrRenderError(url, options = {}) {
    try {
        const resp = await fetch(url, options);

        // First check: did middleware tell us to redirect via header?
        if (!resp.ok) {
            const redirectFromHeader = resp.headers.get("X-Error-Redirect");
            if (redirectFromHeader) {
                window.location.href = redirectFromHeader;
                return; // bail after redirect
            }

            // No header? Try to parse the JSON body
            const contentType = resp.headers.get("Content-Type") || "";
            if (contentType.includes("application/json")) {
                try {
                    const data = await resp.json();
                    if (data.redirectUrl) {
                        window.location.href = data.redirectUrl;
                        return;
                    }
                } catch (err) {
                    console.warn("Failed to parse error JSON:", err);
                }
            }

            // Last resort: hard‑fail to a generic error page
            window.location.href = "/Error";
            return;
        }

        // OK response — parse as JSON if that’s expected
        const contentType = resp.headers.get("Content-Type") || "";
        if (contentType.includes("application/json")) {
            return await resp.json();
        }
        return await resp.text();

    } catch (networkErr) {
        console.error("Network error during fetch:", networkErr);
        window.location.href = "/Error";
    }
}
