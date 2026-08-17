export function postKindLabel(kind: string | null): { text: string; color: string } | null {
  if (kind === "Problem") return { text: "Problem", color: "#b8860b" };
  if (kind === "Emergency") return { text: "Hitan slučaj", color: "#d13438" };
  return null;
}

export function intentKindLabel(kind: string) {
  if (kind === "Ban") return "Izbacivanje člana";
  if (kind === "ManagerElection") return "Izbor upravnika";
  if (kind === "Mute") return "Utišavanje člana";
  if (kind === "PostRating") return "Ocena objave";
  return kind;
}

export function ratingZoneLabel(zone: string) {
  if (zone === "Green") return "prihvaćeno";
  if (zone === "Yellow") return "podeljeno";
  if (zone === "Red") return "odbijeno";
  return zone;
}

export function ratingZoneColor(zone: string): { text: string; background: string } {
  if (zone === "Green") return { text: "#1d8a4a", background: "#e3f5ea" };
  if (zone === "Yellow") return { text: "#8a6d1d", background: "#fbf3d9" };
  if (zone === "Red") return { text: "#d13438", background: "#fbe3e4" };
  return { text: "#6b7280", background: "#eceef2" };
}

export function intentStatusLabel(status: string) {
  if (status === "Open") return "glasanje u toku";
  if (status === "Accepted") return "prihvaćena";
  if (status === "Rejected") return "odbijena";
  if (status === "Expired") return "istekla";
  return status;
}

export function helpStatusLabel(status: string | null) {
  if (status === "Active") return "u toku";
  if (status === "Concluded") return "zaključena";
  if (status === "HelperResigned") return "pomagač odustao";
  return status ?? "";
}

export function helpStatusColor(status: string | null): { text: string; background: string } {
  if (status === "Active") return { text: "#1d8a4a", background: "#e3f5ea" };
  if (status === "Concluded") return { text: "#1f4fb0", background: "#e6eefc" };
  if (status === "HelperResigned") return { text: "#6b7280", background: "#eceef2" };
  return { text: "#6b7280", background: "#eceef2" };
}

export function formatDateTime(value: string) {
  return new Date(value).toLocaleString("sr-RS", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}
