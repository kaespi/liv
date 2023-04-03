#pragma once

#include "IDir.h"

#include <QDir>

class DirWrapper : public IDir
{
  public:
    DirWrapper() = default;
    virtual ~DirWrapper() = default;

    virtual void setPath(const QString& path) override;
    virtual void setCurrentPath() override;
    virtual QString absolutePath() const override;
    virtual QStringList entryList(const QStringList& nameFilters, QDir::Filters filters,
                                  QDir::SortFlags sort) const override;

  private:
    QDir m_dir{};
};
