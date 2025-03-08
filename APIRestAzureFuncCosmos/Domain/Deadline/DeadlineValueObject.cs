using Domain.Deadline.Exceptions;
using Shared;
using Shared.Consts;
using Shared.Interfaces;

namespace Domain.Deadline;

public class DeadlineValueObject
{
    public DateTime? Value { get; }

    public DeadlineValueObject(DateTime? deadlineDate, DateTime createdDate, IDateTimeProvider dateTimeProvider)
    {
        dateTimeProvider ??= new DateTimeProvider();

        if (deadlineDate.HasValue && deadlineDate.Value < dateTimeProvider.GetUTCNow())
            throw new DeadlineException(Constants.VALIDATION_TASK_DEADLINE_NOT_PAST);

        if (deadlineDate.HasValue && deadlineDate.Value < createdDate)
            throw new DeadlineException(Constants.VALIDATION_TASK_CANNOT_BEFORE_CREATEAT);


        Value = deadlineDate;
    }
}