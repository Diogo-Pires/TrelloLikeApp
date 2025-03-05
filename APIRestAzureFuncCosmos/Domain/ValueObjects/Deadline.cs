using Shared;
using Shared.Consts;
using Shared.Exceptions;
using Shared.Interfaces;

namespace Domain.ValueObjects;

public class Deadline
{
    public DateTime? Value { get; }

    public Deadline(DateTime? deadlineDate, DateTime createdDate, IDateTimeProvider dateTimeProvider)
    {
        dateTimeProvider ??= new DateTimeProvider();

        if (deadlineDate.HasValue && deadlineDate.Value < dateTimeProvider.GetUTCNow())
            throw new DomainException(Constants.VALIDATION_TASK_DEADLINE_NOT_PAST);

        if (deadlineDate.HasValue && deadlineDate.Value < createdDate)
            throw new DomainException(Constants.VALIDATION_TASK_CANNOT_BEFORE_CREATEAT);


        Value = deadlineDate;
    }
}