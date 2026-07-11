import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { GraduationCap, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import { Link } from 'react-router-dom';

const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [remember, setRemember] = useState(false);
  const { login } = useAuthStore();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!email || !password) { toast.error('Please fill in all fields'); return; }
    setLoading(true);
    const success = await login(email, password);
    setLoading(false);
    if (success) {
      toast.success('Welcome back!');
      navigate('/dashboard');
    } else {
      toast.error('Invalid credentials. Try admin@college.edu');
    }
  };

  return (
    <div className="min-h-screen flex">
      <div className="hidden lg:flex lg:w-1/2 bg-primary items-center justify-center p-12 relative overflow-hidden">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_hsl(var(--primary))_0%,_hsl(220_70%_35%)_100%)]" />
        <div className="relative z-10 text-center space-y-6 max-w-md">
          <div className="mx-auto rounded-2xl bg-primary-foreground/10 p-4 w-fit backdrop-blur-sm">
            <GraduationCap className="h-12 w-12 text-primary-foreground" />
          </div>
          <h1 className="text-3xl font-bold text-primary-foreground">AttendanceMS</h1>
          <p className="text-primary-foreground/70 text-sm leading-relaxed">
            Streamline attendance tracking across your institution. Real-time analytics, automated blacklisting, and comprehensive reporting.
          </p>
          <div className="grid grid-cols-3 gap-4 pt-4">
            {[['500+', 'Students'], ['50+', 'Faculty'], ['99.9%', 'Uptime']].map(([val, label]) => (
              <div key={label} className="rounded-xl bg-primary-foreground/10 p-3 backdrop-blur-sm">
                <p className="text-lg font-bold text-primary-foreground">{val}</p>
                <p className="text-[10px] text-primary-foreground/60">{label}</p>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="flex-1 flex items-center justify-center p-6 bg-background">
        <div className="w-full max-w-sm space-y-6">
          <div className="lg:hidden flex items-center gap-3 justify-center mb-4">
            <div className="rounded-lg bg-primary p-2"><GraduationCap className="h-5 w-5 text-primary-foreground" /></div>
            <h1 className="text-xl font-bold">AttendanceMS</h1>
          </div>
          <div className="space-y-1.5">
            <h2 className="text-2xl font-bold">Welcome back</h2>
            <p className="text-sm text-muted-foreground">Sign in to your account to continue</p>
          </div>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" placeholder="admin@college.edu" value={email} onChange={e => setEmail(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input id="password" type="password" placeholder="••••••••" value={password} onChange={e => setPassword(e.target.value)} />
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Checkbox id="remember" checked={remember} onCheckedChange={(c) => setRemember(!!c)} />
                <Label htmlFor="remember" className="text-sm font-normal">Remember me</Label>
              </div>
              <Link to="/forgot-password" className="text-sm text-primary hover:underline">Forgot password?</Link>
            </div>
            <Button type="submit" className="w-full" disabled={loading}>
              {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Sign In
            </Button>
          </form>
          <div className="space-y-2 pt-2">
            <p className="text-xs text-center text-muted-foreground">Quick login as:</p>
            <div className="flex gap-2">
              {['admin', 'faculty', 'student'].map(role => (
                <Button key={role} variant="outline" size="sm" className="flex-1 capitalize text-xs" onClick={() => { setEmail(`${role}@college.edu`); setPassword('demo'); }}>
                  {role}
                </Button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
