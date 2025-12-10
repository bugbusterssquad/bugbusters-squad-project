import React from "react";

export default function Footer() {
  return (
    <footer className="bg-white border-t mt-8">
      <div className="container mx-auto px-4 py-6 flex flex-col md:flex-row items-center justify-between">
        <div className="text-sm text-gray-600">© {new Date().getFullYear()} Clubs Platform - Tüm hakları saklıdır.</div>
        <div className="flex gap-4 mt-3 md:mt-0">
          <a className="text-sm text-gray-600 hover:text-accent">Destek</a>
          <a className="text-sm text-gray-600 hover:text-accent">Gizlilik</a>
        </div>
      </div>
    </footer>
  );
}
