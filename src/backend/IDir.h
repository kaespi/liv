#pragma once

#include <QDir>
#include <QString>

class IDir
{
  public:
    virtual ~IDir() = default;
    virtual void setPath(const QString& path) = 0;
    virtual void setCurrentPath() = 0;
    virtual QString absolutePath() const = 0;
    virtual QStringList entryList(const QStringList& nameFilters, QDir::Filters filters,
                                  QDir::SortFlags sort) const = 0;
};
