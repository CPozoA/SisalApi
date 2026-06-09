using System;
using System.Collections.Generic;
using System.Text;

namespace Sisal.Application.Common.Messaging
{
    public interface ICommand<TResult>;

    public interface IQuery<TResult>;

    public interface ICommandHandler<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    }

    public interface IQueryHandler<in TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
    }

    public interface IDispatcher
    {
        Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

        Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
    }
}
