import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { GraduationCap, ArrowLeft, Loader2, CheckCircle } from 'lucide-react';
import { toast } from 'sonner';

const ForgotPasswordPage = () => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!email) { toast.error('Please enter your email'); return; }
    setLoading(true);
    await new Promise(r => setTimeout(r, 1000));
    setLoading(false);
    setSent(true);
    toast.success('Reset link sent!');
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-6 bg-background">
      <div className="w-full max-w-sm space-y-6">
        <div className="flex items-center gap-3 justify-center">
          <div className="rounded-lg bg-primary p-2"><GraduationCap className="h-5 w-5 text-primary-foreground" /></div>
          <h1 className="text-xl font-bold">AttendanceMS</h1>
        </div>
        {sent ? (
          <div className="text-center space-y-4 animate-fade-in">
            <div className="mx-auto rounded-full bg-success/10 p-4 w-fit"><CheckCircle className="h-8 w-8 text-success" /></div>
            <h2 className="text-xl font-bold">Check your email</h2>
            <p className="text-sm text-muted-foreground">We've sent a password reset link to <strong>{email}</strong></p>
            <Link to="/login"><Button variant="outline" className="mt-4"><ArrowLeft className="mr-2 h-4 w-4" />Back to login</Button></Link>
          </div>
        ) : (
          <>
            <div className="space-y-1.5">
              <h2 className="text-2xl font-bold">Forgot password?</h2>
              <p className="text-sm text-muted-foreground">Enter your email and we'll send you a reset link</p>
            </div>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input id="email" type="email" placeholder="your@college.edu" value={email} onChange={e => setEmail(e.target.value)} />
              </div>
              <Button type="submit" className="w-full" disabled={loading}>
                {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}Send Reset Link
              </Button>
            </form>
            <Link to="/login" className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground justify-center">
              <ArrowLeft className="h-4 w-4" />Back to login
            </Link>
          </>
        )}
      </div>
    </div>
  );
};

export default ForgotPasswordPage;
