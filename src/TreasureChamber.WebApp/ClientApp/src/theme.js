const THEME_KEY = 'tc-theme'

// Vite 会把两张主题 CSS 作为独立资源输出并给出 URL
const themeUrls = import.meta.glob('./assets/themes/*.css', { eager: true, query: '?url', import: 'default' })

export const themeNames = ['light', 'dark']

export function currentTheme() {
  const saved = localStorage.getItem(THEME_KEY)
  if (saved === 'light' || saved === 'dark') return saved
  return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export function applyTheme(theme) {
  const file = theme === 'dark' ? 'lara-dark-blue.css' : 'lara-light-blue.css'
  const url = themeUrls[`./assets/themes/${file}`]
  let link = document.getElementById('tc-theme-link')
  if (!link) {
    link = document.createElement('link')
    link.id = 'tc-theme-link'
    link.rel = 'stylesheet'
    document.head.appendChild(link)
  }
  link.href = url
  document.documentElement.setAttribute('data-theme', theme)
  localStorage.setItem(THEME_KEY, theme)
}

export function toggleTheme() {
  const next = currentTheme() === 'dark' ? 'light' : 'dark'
  applyTheme(next)
  return next
}
