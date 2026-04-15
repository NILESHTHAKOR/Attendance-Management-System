export const mockStudents = [
  { id: 's1', name: 'Rahul Verma', email: 'rahul@college.edu', rollNo: 'BCA-001', classId: 'c1', className: 'BCA Sem 4', section: 'A', phone: '9876543210', attendance: 82, status: 'active' },
  { id: 's2', name: 'Priya Singh', email: 'priya@college.edu', rollNo: 'BCA-002', classId: 'c1', className: 'BCA Sem 4', section: 'A', phone: '9876543211', attendance: 68, status: 'warning' },
  { id: 's3', name: 'Amit Patel', email: 'amit@college.edu', rollNo: 'BCA-003', classId: 'c1', className: 'BCA Sem 4', section: 'A', phone: '9876543212', attendance: 45, status: 'blacklisted' },
  { id: 's4', name: 'Sneha Gupta', email: 'sneha@college.edu', rollNo: 'BCA-004', classId: 'c1', className: 'BCA Sem 4', section: 'B', phone: '9876543213', attendance: 91, status: 'active' },
  { id: 's5', name: 'Vikram Reddy', email: 'vikram@college.edu', rollNo: 'MCA-001', classId: 'c2', className: 'MCA Sem 2', section: 'A', phone: '9876543214', attendance: 73, status: 'warning' },
  { id: 's6', name: 'Neha Sharma', email: 'neha@college.edu', rollNo: 'MCA-002', classId: 'c2', className: 'MCA Sem 2', section: 'A', phone: '9876543215', attendance: 48, status: 'blacklisted' },
  { id: 's7', name: 'Arjun Nair', email: 'arjun@college.edu', rollNo: 'BCA-005', classId: 'c1', className: 'BCA Sem 4', section: 'B', phone: '9876543216', attendance: 88, status: 'active' },
  { id: 's8', name: 'Kavita Joshi', email: 'kavita@college.edu', rollNo: 'MCA-003', classId: 'c2', className: 'MCA Sem 2', section: 'A', phone: '9876543217', attendance: 55, status: 'warning' },
  { id: 's9', name: 'Ravi Tiwari', email: 'ravi@college.edu', rollNo: 'BCA-006', classId: 'c3', className: 'BCA Sem 6', section: 'A', phone: '9876543218', attendance: 42, status: 'blacklisted' },
  { id: 's10', name: 'Meera Das', email: 'meera@college.edu', rollNo: 'BCA-007', classId: 'c3', className: 'BCA Sem 6', section: 'A', phone: '9876543219', attendance: 95, status: 'active' },
  { id: 's11', name: 'Saurabh Yadav', email: 'saurabh@college.edu', rollNo: 'MCA-004', classId: 'c2', className: 'MCA Sem 2', section: 'B', phone: '9876543220', attendance: 77, status: 'active' },
  { id: 's12', name: 'Pooja Mishra', email: 'pooja@college.edu', rollNo: 'BCA-008', classId: 'c1', className: 'BCA Sem 4', section: 'A', phone: '9876543221', attendance: 63, status: 'warning' },
];

export const mockClasses = [
  { id: 'c1', name: 'BCA', section: 'A', semester: 4, department: 'Computer Applications', studentCount: 45, subjects: ['sub1', 'sub2', 'sub3'], facultyId: '2', facultyName: 'Prof. Anita Sharma' },
  { id: 'c2', name: 'MCA', section: 'A', semester: 2, department: 'Computer Applications', studentCount: 38, subjects: ['sub4', 'sub5'], facultyId: '2', facultyName: 'Prof. Anita Sharma' },
  { id: 'c3', name: 'BCA', section: 'A', semester: 6, department: 'Computer Applications', studentCount: 42, subjects: ['sub6', 'sub7'], facultyName: 'Prof. Suresh Iyer' },
  { id: 'c4', name: 'BSc IT', section: 'A', semester: 3, department: 'Information Technology', studentCount: 50, subjects: ['sub8'], facultyName: 'Prof. Meena Kapoor' },
];

export const mockSubjects = [
  { id: 'sub1', name: 'Data Structures', code: 'CS301', classId: 'c1', className: 'BCA Sem 4', facultyId: '2', facultyName: 'Prof. Anita Sharma' },
  { id: 'sub2', name: 'Database Management', code: 'CS302', classId: 'c1', className: 'BCA Sem 4', facultyId: '2', facultyName: 'Prof. Anita Sharma' },
  { id: 'sub3', name: 'Web Development', code: 'CS303', classId: 'c1', className: 'BCA Sem 4', facultyId: '3', facultyName: 'Prof. Vikash Gupta' },
  { id: 'sub4', name: 'Advanced Java', code: 'MCA201', classId: 'c2', className: 'MCA Sem 2', facultyId: '2', facultyName: 'Prof. Anita Sharma' },
  { id: 'sub5', name: 'Software Engineering', code: 'MCA202', classId: 'c2', className: 'MCA Sem 2', facultyId: '4', facultyName: 'Prof. Ritu Malhotra' },
  { id: 'sub6', name: 'Machine Learning', code: 'CS601', classId: 'c3', className: 'BCA Sem 6', facultyId: '5', facultyName: 'Prof. Suresh Iyer' },
  { id: 'sub7', name: 'Cloud Computing', code: 'CS602', classId: 'c3', className: 'BCA Sem 6', facultyId: '5', facultyName: 'Prof. Suresh Iyer' },
  { id: 'sub8', name: 'Networking', code: 'IT301', classId: 'c4', className: 'BSc IT Sem 3', facultyId: '6', facultyName: 'Prof. Meena Kapoor' },
];

export const generateAttendanceRecords = (classId, date) => {
  const students = mockStudents.filter(s => s.classId === classId);
  return students.map((s, i) => ({
    id: `att-${s.id}-${date}`,
    studentId: s.id,
    studentName: s.name,
    rollNo: s.rollNo,
    classId,
    subjectId: 'sub1',
    date,
    status: i % 5 === 0 ? 'absent' : i % 7 === 0 ? 'late' : 'present',
  }));
};

export const monthlyAttendanceData = [
  { month: 'Sep', attendance: 88 },
  { month: 'Oct', attendance: 85 },
  { month: 'Nov', attendance: 79 },
  { month: 'Dec', attendance: 82 },
  { month: 'Jan', attendance: 76 },
  { month: 'Feb', attendance: 80 },
  { month: 'Mar', attendance: 78 },
  { month: 'Apr', attendance: 74 },
];

export const subjectWiseAttendance = [
  { subject: 'Data Structures', attendance: 85, total: 40, present: 34 },
  { subject: 'Database Mgmt', attendance: 78, total: 38, present: 30 },
  { subject: 'Web Dev', attendance: 72, total: 36, present: 26 },
  { subject: 'Adv. Java', attendance: 90, total: 42, present: 38 },
];

export const heatmapData = {};
const startDate = new Date('2026-01-01');
for (let i = 0; i < 103; i++) {
  const d = new Date(startDate);
  d.setDate(d.getDate() + i);
  if (d.getDay() !== 0) {
    heatmapData[d.toISOString().split('T')[0]] = Math.floor(Math.random() * 100);
  }
}
