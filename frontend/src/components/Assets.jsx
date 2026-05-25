import { useEffect, useState, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { getDevices, createDevice, updateDevice, disposeDevice, getUsers } from '../api';
import { getStoredUser } from '../utils/user';
import { PERMISSIONS } from '../utils/permissions';
import { useToast } from './Toast';
import { getApiError } from '../utils/apiError';

const STATUS_MAP = {
  AVAILABLE: { label: 'Có sẵn', badge: 'badge-warning' },
  ASSIGNED: { label: 'Đã cấp phát', badge: 'badge-success' },
  DISPOSED: { label: 'Thanh lý', badge: 'badge-neutral' },
  MAINTENANCE: { label: 'Bảo trì', badge: 'badge-info' },
};

const emptyForm = {
  name: '',
  type: 'Laptop',
  originalCost: '',
  status: 'AVAILABLE',
  purchaseDate: '',
};

export default function Assets() {
  const user = getStoredUser();
  const role = user?.role;
  const isAdmin = PERMISSIONS.manageDevices(role);
  const isEmployee = role === 'EMPLOYEE';

  const [searchParams] = useSearchParams();
  const q = searchParams.get('q') || '';

  const [devices, setDevices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [formData, setFormData] = useState(emptyForm);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [userFilter, setUserFilter] = useState('');
  const [users, setUsers] = useState([]);
  const { showToast } = useToast();

  const fetchDevices = useCallback(async (keyword = searchTerm) => {
    setLoading(true);
    try {
      const res = await getDevices(keyword, statusFilter);
      setDevices(res.data.data || []);
    } catch (err) {
      showToast(getApiError(err, 'Không tải được danh sách thiết bị'), 'error');
      setDevices([]);
    } finally {
      setLoading(false);
    }
  }, [searchTerm, statusFilter, showToast]);

  useEffect(() => {
    if (!isEmployee) {
      getUsers().then((res) => setUsers(res.data.data || [])).catch(() => {});
    }
  }, [isEmployee]);

  useEffect(() => {
    if (q) {
      setSearchTerm(q);
      fetchDevices(q);
    } else {
      fetchDevices();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [q, statusFilter]);

  const openCreate = () => {
    setEditingId(null);
    setFormData(emptyForm);
    setShowForm(true);
  };

  const openEdit = (d) => {
    setEditingId(d.id);
    setFormData({
      name: d.name || '',
      type: d.type || 'Laptop',
      originalCost: d.originalCost ?? '',
      status: d.status || 'AVAILABLE',
      purchaseDate: d.purchaseDate ? d.purchaseDate.slice(0, 10) : '',
    });
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const payload = {
      name: formData.name.trim(),
      type: formData.type,
      originalCost: Number(formData.originalCost),
      status: formData.status,
      purchaseDate: formData.purchaseDate || null,
    };
    try {
      if (editingId) {
        await updateDevice(editingId, payload);
        showToast('Cập nhật thiết bị thành công!', 'success');
      } else {
        await createDevice(payload);
        showToast('Thêm thiết bị thành công!', 'success');
      }
      setShowForm(false);
      setFormData(emptyForm);
      setEditingId(null);
      if (!editingId) setSearchTerm('');
      await fetchDevices(editingId ? searchTerm : '');
    } catch (err) {
      showToast(getApiError(err, 'Lưu thiết bị thất bại'), 'error');
    }
  };

  const handleDispose = async (id, name) => {
    if (!window.confirm(`Chuyển "${name}" sang Thanh lý? Dữ liệu vẫn lưu trong hệ thống.`)) return;
    try {
      await disposeDevice(id);
      showToast('Đã chuyển sang Thanh lý', 'success');
      await fetchDevices();
    } catch (err) {
      showToast(getApiError(err, 'Thanh lý thất bại'), 'error');
    }
  };

  const filteredDevices = devices.filter((d) => {
    const matchType = !typeFilter || d.type === typeFilter;
    const matchUser = !userFilter || d.assignedUserId === Number(userFilter);
    return matchType && matchUser;
  });

  return (
    <div className="page active" id="page-devices">
      <div className="action-bar">
        <div className="filter-bar">
          <input
            type="text"
            className="search-input"
            placeholder={isEmployee ? 'Tìm thiết bị của tôi...' : 'Tìm theo tên thiết bị...'}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && fetchDevices()}
          />
          {!isEmployee && (
            <>
              <select className="filter-select" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
                <option value="">Tất cả trạng thái</option>
                <option value="AVAILABLE">Có sẵn</option>
                <option value="ASSIGNED">Đã cấp phát</option>
                <option value="MAINTENANCE">Bảo trì</option>
                <option value="DISPOSED">Thanh lý</option>
              </select>

              <select className="filter-select" value={typeFilter} onChange={(e) => setTypeFilter(e.target.value)}>
                <option value="">Tất cả loại</option>
                <option value="Laptop">Laptop</option>
                <option value="Monitor">Monitor</option>
                <option value="Server">Test Server</option>
              </select>

              <select className="filter-select" value={userFilter} onChange={(e) => setUserFilter(e.target.value)}>
                <option value="">Tất cả người dùng</option>
                {users.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.fullName}
                  </option>
                ))}
              </select>
            </>
          )}
          <button type="button" className="btn btn-secondary btn-sm" onClick={() => {
            fetchDevices();
            setTypeFilter('');
            setUserFilter('');
          }}>
            <i className="ti ti-refresh" /> Tải lại
          </button>
        </div>
        {isAdmin && (
          <button type="button" className="btn btn-primary" onClick={openCreate}>
            <i className="ti ti-plus" /> Thêm thiết bị
          </button>
        )}
      </div>

      <div className="card">
        <div className="card-body" style={{ padding: 0 }}>
          <div className="table-wrap">
            {loading ? (
              <div style={{ padding: '20px', color: 'var(--text3)' }}>Đang tải...</div>
            ) : (
              <table>
                <thead>
                  <tr>
                    <th>Mã TB</th>
                    <th>Tên thiết bị</th>
                    <th>Loại</th>
                    {!isEmployee && <th>Người dùng</th>}
                    <th>Nguyên giá</th>
                    <th>Trạng thái</th>
                    {isAdmin && <th>Thao tác</th>}
                  </tr>
                </thead>
                <tbody>
                  {filteredDevices.map((d) => {
                    const st = STATUS_MAP[d.status] || STATUS_MAP.AVAILABLE;
                    return (
                      <tr key={d.id}>
                        <td className="mono">TB-{String(d.id).padStart(3, '0')}</td>
                        <td><div style={{ fontWeight: 500 }}>{d.name}</div></td>
                        <td>{d.type}</td>
                        {!isEmployee && (
                          <td>{d.assignedUser?.fullName || <span style={{ color: 'var(--text3)' }}>—</span>}</td>
                        )}
                        <td className="mono">{d.originalCost?.toLocaleString('vi-VN')}đ</td>
                        <td><span className={`badge ${st.badge}`}>{st.label}</span></td>
                        {isAdmin && (
                          <td>
                            <div style={{ display: 'flex', gap: '6px' }}>
                              {d.status !== 'DISPOSED' && (
                                <>
                                  <button type="button" className="btn btn-secondary btn-sm" onClick={() => openEdit(d)} title="Sửa">
                                    <i className="ti ti-edit" />
                                  </button>
                                  <button type="button" className="btn btn-danger btn-sm" onClick={() => handleDispose(d.id, d.name)} title="Thanh lý">
                                    <i className="ti ti-archive" />
                                  </button>
                                </>
                              )}
                            </div>
                          </td>
                        )}
                      </tr>
                    );
                  })}
                  {filteredDevices.length === 0 && (
                    <tr>
                      <td colSpan={isAdmin ? 7 : isEmployee ? 5 : 6} style={{ textAlign: 'center', color: 'var(--text3)' }}>
                        Không có thiết bị
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            )}
          </div>
        </div>
      </div>

      {showForm && (
        <div className="modal-overlay open" onClick={(e) => e.target === e.currentTarget && setShowForm(false)}>
          <div className="modal">
            <div className="modal-header">
              <div className="modal-title">{editingId ? 'Cập nhật thiết bị' : 'Thêm thiết bị mới'}</div>
              <div className="modal-close" onClick={() => setShowForm(false)}><i className="ti ti-x" /></div>
            </div>
            <div className="modal-body">
              <form id="deviceForm" onSubmit={handleSubmit}>
                <div className="form-group">
                  <div className="form-label">Tên thiết bị *</div>
                  <input className="form-input" required value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} />
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <div className="form-label">Loại *</div>
                    <select className="form-select" value={formData.type} onChange={(e) => setFormData({ ...formData, type: e.target.value })}>
                      <option value="Laptop">Laptop</option>
                      <option value="Monitor">Monitor</option>
                      <option value="Server">Test Server</option>
                    </select>
                  </div>
                  <div className="form-group">
                    <div className="form-label">Nguyên giá *</div>
                    <input className="form-input" type="number" required min="0" value={formData.originalCost} onChange={(e) => setFormData({ ...formData, originalCost: e.target.value })} />
                  </div>
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <div className="form-label">Ngày mua</div>
                    <input className="form-input" type="date" value={formData.purchaseDate} onChange={(e) => setFormData({ ...formData, purchaseDate: e.target.value })} />
                  </div>
                  <div className="form-group">
                    <div className="form-label">Trạng thái</div>
                    <select className="form-select" value={formData.status} onChange={(e) => setFormData({ ...formData, status: e.target.value })}>
                      <option value="AVAILABLE">Có sẵn</option>
                      <option value="ASSIGNED">Đã cấp phát</option>
                      <option value="MAINTENANCE">Bảo trì</option>
                    </select>
                  </div>
                </div>
              </form>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={() => setShowForm(false)}>Hủy</button>
              <button type="submit" form="deviceForm" className="btn btn-primary">
                <i className="ti ti-device-floppy" /> Lưu
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
