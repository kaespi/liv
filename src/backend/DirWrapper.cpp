#include "DirWrapper.h"

void DirWrapper::setPath(const QString& path)
{
    m_dir = QDir(path);
}

void DirWrapper::setCurrentPath()
{
    m_dir = QDir(QDir::currentPath());
}

QString DirWrapper::absolutePath() const
{
    return m_dir.absolutePath();
}

QStringList DirWrapper::entryList(const QStringList& nameFilters, QDir::Filters filters,
                                  QDir::SortFlags sort) const
{
    return m_dir.entryList(nameFilters, filters, sort);
}
