export async function setUnlockCookie(apiKey) {
  await fetch("/api/unlock", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "same-origin",
    body: JSON.stringify({ apiKey }),
  });
}
