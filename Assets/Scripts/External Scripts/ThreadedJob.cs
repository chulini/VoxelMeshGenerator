using UnityEngine;
using System.Collections;

/// <summary>
/// Template class to make a job using a thread. 
/// </summary>
public class ThreadedJob
{
	private bool m_IsDone = false;
	private object m_Handle = new object();
	private System.Threading.Thread m_Thread = null;
	/// <summary>
	/// Is set to true when the job is finished
	/// </summary>
	public bool IsDone
	{
		get
		{
			bool tmp;
			lock (m_Handle)
			{
				tmp = m_IsDone;
			}
			return tmp;
		}
		set
		{
			lock (m_Handle)
			{
				m_IsDone = value;
			}
		}
	}

	public virtual void Start()
	{
		m_Thread = new System.Threading.Thread(Run);
		m_Thread.Start();
	}
	public virtual void Abort()
	{
        if(m_Thread != null)
		    m_Thread.Abort();
	}
	public virtual void Interrupt(){
        if (m_Thread != null)
            m_Thread.Interrupt ();
	}

	protected virtual void ThreadFunction() { }

	protected virtual void OnFinished() { }

	public virtual bool Update()
	{
		if (IsDone)
		{
			OnFinished();
			return true;
		}
		return false;
	}
	public IEnumerator WaitFor()
	{
		while(!Update())
		{
			yield return null;
		}

	}
	public void Reset(){
		m_IsDone = false;
	}
	private void Run()
	{
		ThreadFunction();
		IsDone = true;
	}
}

