/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
      "./Pages/**/*.cshtml",
      "./Views/**/*.cshtml",
      "./**/*.razor"
    ],
    safelist: [
      'text-yellow-400',
      'text-gray-400',
      'text-2xl'
    ],
    theme: {
      extend: {},
    },
    plugins: [],
  }