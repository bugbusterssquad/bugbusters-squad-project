import { useState } from "react";
import { Link, useNavigate } from "react-router";
import { clearAuth, getUser } from "../services/auth";

export default function NavBar() {
  const [open, setOpen] = useState(false);
  const user = getUser();
  const navigate = useNavigate();

  function logout() {
    clearAuth();
    navigate("/");
  }

  return (
    <header className="bg-white shadow">
      <div className="container mx-auto px-4 py-3 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-3">
          <div className="bg-accent w-10 h-10 rounded flex items-center justify-center text-white font-bold">
            C
          </div>
          <div className="hidden sm:block">
            <h1 className="text-lg font-semibold">Clubs</h1>
            <p className="text-sm text-gray-500">Öğrenci Kulüpleri Platformu</p>
          </div>
        </Link>

        <nav className="hidden md:flex items-center gap-4">
          <Link to="/" className="hover:text-accent">Kulüpler</Link>
          <Link to="/events" className="hover:text-accent">Etkinlikler</Link>
          {!user && (
            <>
              <Link to="/login" className="text-sm px-4 py-2 border rounded">Giriş</Link>
              <Link to="/register" className="text-sm px-4 py-2 bg-accent text-white rounded">Kayıt Ol</Link>
            </>
          )}
          {user && (
            <>
              {(user.role === "ClubAdmin" || user.role === "SuperAdmin") && (
                <Link to="/admin" className="text-sm px-3 py-2 border rounded">Admin</Link>
              )}
              {(user.role === "SksAdmin" || user.role === "SuperAdmin") && (
                <Link to="/sks" className="text-sm px-3 py-2 border rounded">SKS</Link>
              )}
              <Link to="/profile" className="text-sm px-3 py-2 border rounded">{user.name}</Link>
              <button onClick={logout} className="text-sm px-3 py-2 border rounded">Çıkış</button>
            </>
          )}
        </nav>

        {/* mobile */}
        <div className="md:hidden flex items-center">
          <button onClick={() => setOpen(v => !v)} className="p-2 rounded-md border">
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              {open ? (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
              ) : (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 6h16M4 12h16M4 18h16" />
              )}
            </svg>
          </button>
        </div>
      </div>

      {open && (
        <div className="md:hidden bg-white border-t">
          <div className="px-4 py-3 flex flex-col gap-2">
            <Link to="/" onClick={() => setOpen(false)} className="py-2">Kulüpler</Link>
            <Link to="/events" onClick={() => setOpen(false)} className="py-2">Etkinlikler</Link>
            {!user && <>
              <Link to="/login" onClick={() => setOpen(false)} className="py-2">Giriş</Link>
              <Link to="/register" onClick={() => setOpen(false)} className="py-2">Kayıt Ol</Link>
            </>}
            {user && <>
              {(user.role === "ClubAdmin" || user.role === "SuperAdmin") && (
                <Link to="/admin" onClick={() => setOpen(false)} className="py-2">Admin</Link>
              )}
              {(user.role === "SksAdmin" || user.role === "SuperAdmin") && (
                <Link to="/sks" onClick={() => setOpen(false)} className="py-2">SKS</Link>
              )}
              <Link to="/profile" onClick={() => setOpen(false)} className="py-2">{user.name}</Link>
              <button onClick={() => { clearAuth(); setOpen(false); }} className="py-2 text-left">Çıkış</button>
            </>}
          </div>
        </div>
      )}
    </header>
  );
}
