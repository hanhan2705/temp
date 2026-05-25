import { useEffect, useState, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { getUsers, createUser, updateUser } from '../api';
import { getStoredUser } from '../utils/user';
import { useToast } from './Toast';
import { getApiError } from '../utils/apiError';

const ROLE_OPTIONS = [
  { value: 'EMPLOYEE', label: 'Nhân viên' },
  { value: 'HR', label: 'HR' },
  { value: 'IT_ADMIN', label: 'IT Admin' },
];

const ROLE_LABEL = Object.fromEntries(ROLE_OPTIONS.map((r) => [r.value, r.label]));

const ROLE_BADGE = {
  IT_ADMIN: 'badge-accent',
  HR: 'badge-success',
  EMPLOYEE: 'badge-neutral',
};

const emptyForm = {
  fullName: '',
  email: '',
  role: 'EMPLOYEE',
  password: '',
  status: true,
};

function initials(name) {
  return (name || '?')
    .split(' ')
    .map((w) => w[0])
    .join('')
    .slice(0, 2)
    .toUpperCase();
}

export default function Users() {
  const currentUser = getStoredUser();
  const isAdmin = currentUser?.role === 'IT_ADMIN';

  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const { showToast } = useToast();

  const [searchParams] = useSearchParams();
  const queryParam = searchParams.get('q') || '';

  const fetchUsers = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getUsers(true);
      setUsers(res.data.data || []);
    } catch (err) {
      showToast(getApiError(err, 'Không tải được danh sách người dùng'), 'error');
      setUsers([]);
    } finally {
      setLoading(false);
    }
  }, [showToast]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  useEffect(() => {
    if (queryParam) {
      setSearchTerm(queryParam);
    }
  }, [queryParam]);

  const filtered = users.filter((u) => {
    const q = searchTerm.trim().toLowerCase();
    const matchSearch =
      !q ||
      u.fullName?.toLowerCase().includes(q) ||
      u.email?.toLowerCase().includes(q);
    const matchRole = !roleFilter || u.role === roleFilter;
    return matchSearch && matchRole;
  });

  const openCreate = () => {
    setEditingId(null);
    setForm(emptyForm);
    setShowModal(true);
  };

  const openEdit = (u) => {
    setEditingId(u.id);
    setForm({
      fullName: u.fullName || '',
      email: u.email || '',
      role: u.role || 'EMPLOYEE',
      password: '',
      status: u.status !== false,
    });
    setShowModal(true);
  };

  const handleSubmit = async () => {
    if (!form.fullName.trim() || !form.email.trim()) {
      showToast('Vui lòng điền họ tên và email', 'warning');
      return;
    }
    if (!editingId && (!form.password || form.password.length < 8)) {
      showToast('Mật khẩu tối thiểu 8 ký tự', 'warning');
      return;
    }
    try {
      if (editingId) {
        const payload = {
          fullName: form.fullName.trim(),
          email: form.email.trim(),
          role: form.role,
          status: form.status,
        };
        if (form.password) payload.password = form.password;
        await updateUser(editingId, payload);
        showToast('Cập nhật tài khoản thành công!', 'success');
      } else {
        await createUser({
          fullName: form.fullName.trim(),
          email: form.email.trim(),
          role: form.role,
          password: form.password,
        });
        showToast('Tạo tài khoản thành công!', 'success');
      }
      setShowModal(false);
      setForm(emptyForm);
      setEditingId(null);
      await fetchUsers();
    } catch (err) {
      showToast(getApiError(err, 'Lưu tài khoản thất bại'), 'error');
    }
  };

  return (
    <div className="page active" id="page-users">
      <div className="action-bar">
        <div className="filter-bar">
          <input
            className="search-input"
            placeholder="Tìm tài khoản..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
          <select className="filter-select" value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)}>
            <option value="">Tất cả vai trò</option>
            {ROLE_OPTIONS.map((r) => (
              <option key={r.value} value={r.value}>{r.label}</option>
            ))}
          </select>
          <button type="button" className="btn btn-secondary btn-sm" onClick={fetchUsers}>
            <i className="ti ti-refresh" /> Tải lại
          </button>
        </div>
        {isAdmin && (
          <button type="button" className="btn btn-primary" onClick={openCreate}>
            <i className="ti ti-plus" /> Thêm tài khoản
          </button>
        )}
      </div>
      <div className="card">
        <div className="card-body" style={{ padding: 0 }}>
          {loading ? (
            <div style={{ padding: '20px', color: 'var(--text3)' }}>Đang tải...</div>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Họ tên</th>
                  <th>Email</th>
                  <th>Vai trò</th>
                  <th>Trạng thái</th>
                  {isAdmin && <th>Thao tác</th>}
                </tr>
              </thead>
              <tbody>
                {filtered.map((u) => (
                  <tr key={u.id}>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                        <div
                          style={{
                            width: 32,
                            height: 32,
                            borderRadius: '50%',
                            background: 'var(--accent)',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            fontSize: '12px',
                            fontWeight: 600,
                          }}
                        >
                          {initials(u.fullName)}
                        </div>
                        <div style={{ fontWeight: 500 }}>{u.fullName}</div>
                      </div>
                    </td>
                    <td className="mono">{u.email}</td>
                    <td>
                      <span className={`badge ${ROLE_BADGE[u.role] || 'badge-neutral'}`}>
                        {ROLE_LABEL[u.role] || u.role}
                      </span>
                    </td>
                    <td>
                      <span className={`badge ${u.status ? 'badge-success' : 'badge-danger'}`}>
                        {u.status ? 'Hoạt động' : 'Vô hiệu'}
                      </span>
                    </td>
                    {isAdmin && (
                      <td>
                        <button type="button" className="btn btn-secondary btn-sm" onClick={() => openEdit(u)} title="Sửa">
                          <i className="ti ti-edit" />
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
                {filtered.length === 0 && (
                  <tr>
                    <td colSpan={isAdmin ? 5 : 4} style={{ textAlign: 'center', color: 'var(--text3)' }}>
                      Không có người dùng
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {showModal && isAdmin && (
        <div className="modal-overlay open" onClick={(e) => e.target === e.currentTarget && setShowModal(false)}>
          <div className="modal">
            <div className="modal-header">
              <div className="modal-title">{editingId ? 'Cập nhật tài khoản' : 'Thêm tài khoản người dùng'}</div>
              <div className="modal-close" onClick={() => setShowModal(false)}><i className="ti ti-x" /></div>
            </div>
            <div className="modal-body">
              <div className="form-row">
                <div className="form-group">
                  <div className="form-label">Họ tên *</div>
                  <input
                    className="form-input"
                    value={form.fullName}
                    onChange={(e) => setForm({ ...form, fullName: e.target.value })}
                    placeholder="Nguyễn Văn A"
                  />
                </div>
                <div className="form-group">
                  <div className="form-label">Email *</div>
                  <input
                    className="form-input"
                    type="email"
                    value={form.email}
                    onChange={(e) => setForm({ ...form, email: e.target.value })}
                    placeholder="email@company.com"
                  />
                </div>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <div className="form-label">Vai trò *</div>
                  <select
                    className="form-select"
                    value={form.role}
                    onChange={(e) => setForm({ ...form, role: e.target.value })}
                  >
                    {ROLE_OPTIONS.map((r) => (
                      <option key={r.value} value={r.value}>{r.label}</option>
                    ))}
                  </select>
                </div>
                {editingId && (
                  <div className="form-group">
                    <div className="form-label">Trạng thái</div>
                    <select
                      className="form-select"
                      value={form.status ? 'active' : 'inactive'}
                      onChange={(e) => setForm({ ...form, status: e.target.value === 'active' })}
                    >
                      <option value="active">Hoạt động</option>
                      <option value="inactive">Vô hiệu</option>
                    </select>
                  </div>
                )}
              </div>
              <div className="form-group">
                <div className="form-label">{editingId ? 'Mật khẩu mới (để trống nếu không đổi)' : 'Mật khẩu tạm *'}</div>
                <input
                  className="form-input"
                  type="password"
                  value={form.password}
                  onChange={(e) => setForm({ ...form, password: e.target.value })}
                  placeholder="Tối thiểu 8 ký tự..."
                />
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>Hủy</button>
              <button type="button" className="btn btn-primary" onClick={handleSubmit}>
                <i className="ti ti-device-floppy" /> {editingId ? 'Lưu' : 'Tạo tài khoản'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
