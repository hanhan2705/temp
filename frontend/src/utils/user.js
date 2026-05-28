const ROLE_CONFIG = {
  IT_ADMIN: { name: 'Anh Minh', initials: 'AM', accent: '#6366f1', email: 'admin@company.com' },
  HR: { name: 'Chị Lan', initials: 'CL', accent: '#10b981', email: 'hr@company.com' },
  EMPLOYEE: { name: 'Chị Mai', initials: 'CM', accent: '#f59e0b', email: 'mai@company.com' },
};

export function getStoredUser() {
  try {
    const raw = localStorage.getItem('user');
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

export function getUserDisplay(user) {
  if (!user) return ROLE_CONFIG.IT_ADMIN;
  const role = user.role || 'IT_ADMIN';
  const cfg = ROLE_CONFIG[role] || ROLE_CONFIG.IT_ADMIN;
  // const initials = (user.fullName || cfg.name)
  //   .split(' ')
  //   .map((w) => w[0])
  //   .join('')
  //   .slice(0, 2)
  //   .toUpperCase();
  return {
    name: user.fullName || cfg.name,
    role,
    // initials,
    accent: cfg.accent,
    email: user.email || cfg.email,
  };
}

export { ROLE_CONFIG };
