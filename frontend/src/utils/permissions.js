export const ROLES = {
  IT_ADMIN: 'IT_ADMIN',
  HR: 'HR',
  EMPLOYEE: 'EMPLOYEE',
};

/** Routes each role may access */
export const ROLE_ROUTES = {
  IT_ADMIN: ['/', '/assets', '/requests', '/allocation', '/depreciation', '/history', '/users'],
  HR: ['/', '/assets', '/requests', '/history'],
  EMPLOYEE: ['/', '/assets', '/requests'],
};

export function getRole() {
  try {
    const u = JSON.parse(localStorage.getItem('user') || 'null');
    return u?.role || null;
  } catch {
    return null;
  }
}

export function canAccessRoute(path) {
  const role = getRole();
  if (!role) return false;
  const allowed = ROLE_ROUTES[role] || [];
  return allowed.includes(path);
}

export function getDefaultRoute() {
  const role = getRole();
  return role === ROLES.EMPLOYEE ? '/' : '/';
}

export const PERMISSIONS = {
  manageDevices: (role) => role === ROLES.IT_ADMIN,
  disposeDevice: (role) => role === ROLES.IT_ADMIN,
  viewAllDevices: (role) => role === ROLES.IT_ADMIN || role === ROLES.HR,
  viewMyDevicesOnly: (role) => role === ROLES.EMPLOYEE,
  approveRequests: (role) => role === ROLES.IT_ADMIN,
  sendAllocation: () => true,
  sendRecovery: (role) => role === ROLES.HR || role === ROLES.IT_ADMIN,
  viewDepreciation: (role) => role === ROLES.IT_ADMIN,
  viewUsers: (role) => role === ROLES.IT_ADMIN,
  viewAllocationHistory: (role) => role === ROLES.IT_ADMIN,
  viewOrgDashboard: (role) => role === ROLES.IT_ADMIN || role === ROLES.HR,
};
