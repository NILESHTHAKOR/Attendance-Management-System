import { useState } from 'react';
import { mockStudents } from '@/services/mockData';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { StatusBadge } from '@/components/StatusBadge';
import { Search, Plus, Upload, Download, Pencil, Trash2 } from 'lucide-react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { toast } from 'sonner';

const StudentsPage = () => {
  const [students, setStudents] = useState(mockStudents);
  const [search, setSearch] = useState('');
  const [classFilter, setClassFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editStudent, setEditStudent] = useState(null);

  const filtered = students.filter(s => {
    const matchesSearch = s.name.toLowerCase().includes(search.toLowerCase()) || s.rollNo.toLowerCase().includes(search.toLowerCase());
    const matchesClass = classFilter === 'all' || s.classId === classFilter;
    const matchesStatus = statusFilter === 'all' || s.status === statusFilter;
    return matchesSearch && matchesClass && matchesStatus;
  });

  const handleDelete = (id) => {
    setStudents(prev => prev.filter(s => s.id !== id));
    toast.success('Student removed');
  };

  const handleSave = (e) => {
    e.preventDefault();
    const form = new FormData(e.currentTarget);
    const data = {
      name: form.get('name'),
      email: form.get('email'),
      rollNo: form.get('rollNo'),
      phone: form.get('phone'),
      section: form.get('section'),
    };
    if (editStudent) {
      setStudents(prev => prev.map(s => s.id === editStudent.id ? { ...s, ...data } : s));
      toast.success('Student updated');
    } else {
      const newStudent = { ...data, id: `s${Date.now()}`, classId: 'c1', className: 'BCA Sem 4', attendance: 0, status: 'active' };
      setStudents(prev => [...prev, newStudent]);
      toast.success('Student added');
    }
    setDialogOpen(false);
    setEditStudent(null);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold">Students</h2>
          <p className="text-sm text-muted-foreground">{filtered.length} students found</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm"><Upload className="mr-2 h-4 w-4" />Bulk Upload</Button>
          <Button variant="outline" size="sm"><Download className="mr-2 h-4 w-4" />Export</Button>
          <Dialog open={dialogOpen} onOpenChange={(o) => { setDialogOpen(o); if (!o) setEditStudent(null); }}>
            <DialogTrigger asChild>
              <Button size="sm"><Plus className="mr-2 h-4 w-4" />Add Student</Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader><DialogTitle>{editStudent ? 'Edit' : 'Add'} Student</DialogTitle></DialogHeader>
              <form onSubmit={handleSave} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2"><Label>Name</Label><Input name="name" defaultValue={editStudent?.name} required /></div>
                  <div className="space-y-2"><Label>Roll No</Label><Input name="rollNo" defaultValue={editStudent?.rollNo} required /></div>
                  <div className="space-y-2"><Label>Email</Label><Input name="email" type="email" defaultValue={editStudent?.email} required /></div>
                  <div className="space-y-2"><Label>Phone</Label><Input name="phone" defaultValue={editStudent?.phone} required /></div>
                  <div className="space-y-2"><Label>Section</Label><Input name="section" defaultValue={editStudent?.section || 'A'} required /></div>
                </div>
                <Button type="submit" className="w-full">{editStudent ? 'Update' : 'Add'} Student</Button>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-col sm:flex-row gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input placeholder="Search by name or roll no..." className="pl-9" value={search} onChange={e => setSearch(e.target.value)} />
            </div>
            <Select value={classFilter} onValueChange={setClassFilter}>
              <SelectTrigger className="w-40"><SelectValue placeholder="Class" /></SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Classes</SelectItem>
                <SelectItem value="c1">BCA Sem 4</SelectItem>
                <SelectItem value="c2">MCA Sem 2</SelectItem>
                <SelectItem value="c3">BCA Sem 6</SelectItem>
              </SelectContent>
            </Select>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-36"><SelectValue placeholder="Status" /></SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="active">Active</SelectItem>
                <SelectItem value="warning">Warning</SelectItem>
                <SelectItem value="blacklisted">Blacklisted</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Roll No</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead className="hidden md:table-cell">Class</TableHead>
                  <TableHead className="hidden sm:table-cell">Section</TableHead>
                  <TableHead>Attendance</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map(s => (
                  <TableRow key={s.id}>
                    <TableCell className="font-mono text-xs">{s.rollNo}</TableCell>
                    <TableCell className="font-medium">{s.name}</TableCell>
                    <TableCell className="hidden md:table-cell text-muted-foreground">{s.className}</TableCell>
                    <TableCell className="hidden sm:table-cell">{s.section}</TableCell>
                    <TableCell className={`font-bold ${s.attendance < 50 ? 'text-destructive' : s.attendance < 75 ? 'text-warning' : 'text-success'}`}>{s.attendance}%</TableCell>
                    <TableCell><StatusBadge status={s.status} /></TableCell>
                    <TableCell className="text-right">
                      <div className="flex gap-1 justify-end">
                        <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => { setEditStudent(s); setDialogOpen(true); }}><Pencil className="h-3.5 w-3.5" /></Button>
                        <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => handleDelete(s.id)}><Trash2 className="h-3.5 w-3.5" /></Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default StudentsPage;
