export function postKindLabel(kind: string | null): { text: string; color: string } | null {
  if (kind === "Problem") return { text: "Problem", color: "#b8860b" };
  if (kind === "Emergency") return { text: "Hitan slučaj", color: "#d13438" };
  return null;
}

export function intentKindLabel(kind: string) {
  if (kind === "Ban") return "Izbacivanje člana";
  if (kind === "ManagerElection") return "Izbor upravnika";
  return kind;
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

export function formatDateTime(value: string) {
  return new Date(value).toLocaleString("sr-RS", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}
