import { useAuthStore } from '@/store/authStore';
import AdminDashboard from '@/pages/dashboards/AdminDashboard';
import FacultyDashboard from '@/pages/dashboards/FacultyDashboard';
import StudentDashboard from '@/pages/dashboards/StudentDashboard';

const DashboardPage = () => {
  const { user } = useAuthStore();
  if (user?.role === 'faculty') return <FacultyDashboard />;
  if (user?.role === 'student') return <StudentDashboard />;
  return <AdminDashboard />;
};

export default DashboardPage;
