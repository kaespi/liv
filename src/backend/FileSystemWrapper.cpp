#include "FileSystemWrapper.h"

#include <QFile>

bool FileSystemWrapper::fileExists(const QString& path) const
{
    return QFile::exists(path);
}
