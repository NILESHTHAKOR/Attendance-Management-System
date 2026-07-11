import { mockClasses, mockSubjects } from '@/services/mockData';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Plus, Users, BookOpen } from 'lucide-react';
import { Badge } from '@/components/ui/badge';

const ClassesPage = () => (
  <div className="space-y-6">
    <div className="flex items-center justify-between">
      <div>
        <h2 className="text-2xl font-bold">Classes & Subjects</h2>
        <p className="text-sm text-muted-foreground">Manage classes, subjects, and faculty assignments</p>
      </div>
      <Button size="sm"><Plus className="mr-2 h-4 w-4" />Add Class</Button>
    </div>

    <Tabs defaultValue="classes">
      <TabsList>
        <TabsTrigger value="classes"><BookOpen className="mr-2 h-4 w-4" />Classes</TabsTrigger>
        <TabsTrigger value="subjects"><BookOpen className="mr-2 h-4 w-4" />Subjects</TabsTrigger>
        <TabsTrigger value="timetable">Timetable</TabsTrigger>
      </TabsList>

      <TabsContent value="classes" className="mt-4">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {mockClasses.map(c => (
            <Card key={c.id} className="hover:shadow-md transition-shadow">
              <CardHeader className="pb-3">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-base">{c.name} - Sem {c.semester}</CardTitle>
                  <Badge variant="secondary">Sec {c.section}</Badge>
                </div>
                <p className="text-xs text-muted-foreground">{c.department}</p>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex items-center gap-2 text-sm"><Users className="h-4 w-4 text-muted-foreground" />{c.studentCount} students</div>
                <div className="flex items-center gap-2 text-sm"><BookOpen className="h-4 w-4 text-muted-foreground" />{c.subjects.length} subjects</div>
                {c.facultyName && <p className="text-xs text-muted-foreground">Faculty: {c.facultyName}</p>}
                <Button variant="outline" size="sm" className="w-full">View Details</Button>
              </CardContent>
            </Card>
          ))}
        </div>
      </TabsContent>

      <TabsContent value="subjects" className="mt-4">
        <Card>
          <CardContent className="pt-6">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead>Subject</TableHead>
                  <TableHead>Class</TableHead>
                  <TableHead>Faculty</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {mockSubjects.map(s => (
                  <TableRow key={s.id}>
                    <TableCell className="font-mono text-xs">{s.code}</TableCell>
                    <TableCell className="font-medium">{s.name}</TableCell>
                    <TableCell className="text-muted-foreground">{s.className}</TableCell>
                    <TableCell>{s.facultyName}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </TabsContent>

      <TabsContent value="timetable" className="mt-4">
        <Card>
          <CardContent className="pt-6">
            <div className="grid grid-cols-6 gap-px bg-border rounded-lg overflow-hidden">
              {['Time', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri'].map(d => (
                <div key={d} className="bg-muted p-3 text-center text-xs font-semibold">{d}</div>
              ))}
              {['9:00 AM', '10:00 AM', '11:00 AM', '2:00 PM', '3:00 PM'].map((time, i) => (
                <>
                  <div key={time} className="bg-card p-3 text-xs text-center text-muted-foreground">{time}</div>
                  {[0, 1, 2, 3, 4].map(j => (
                    <div key={`${i}-${j}`} className="bg-card p-2 text-center">
                      {(i + j) % 3 === 0 ? (
                        <div className="rounded bg-primary/10 p-1.5 text-[10px]">
                          <p className="font-medium text-primary">{mockSubjects[((i + j) % mockSubjects.length)].code}</p>
                        </div>
                      ) : null}
                    </div>
                  ))}
                </>
              ))}
            </div>
          </CardContent>
        </Card>
      </TabsContent>
    </Tabs>
  </div>
);

export default ClassesPage;
