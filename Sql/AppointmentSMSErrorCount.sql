select
	appointmentId
	,smsTemplate
	,errorcount = count(*)
from 
	SMSLog s
where
	s.smsIsSent = 0
group by s.appointmentId, s.smsTemplate
having count(0) > 0


