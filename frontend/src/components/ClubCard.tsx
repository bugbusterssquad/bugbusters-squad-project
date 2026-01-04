import { Link } from "react-router";
import type { Club } from "../types";

export default function ClubCard({ club }: { club: Club }) {
  return (
    <Link to={`/clubs/${club.id}`} className="block bg-white shadow-sm rounded-md p-4 hover:shadow-md transition">
      <h3 className="text-lg font-semibold">{club.name}</h3>
      <p className="text-sm text-gray-500 mt-2 line-clamp-2">{club.description ?? "Detay için tıklayın"}</p>
    </Link>
  );
}
