#pragma once

#include "IDir.h"
#include "IFileSystemWalker.h"

#include <QStringList>

class FileSystemWalker : public IFileSystemWalker
{
  public:
    FileSystemWalker(const QString& file, IDir* ptrDir, QList<QByteArray>& fileTypes);
    virtual ~FileSystemWalker() = default;

    QString getCurrentFile() const override;
    QString getNextFile() override;
    QString getPrevFile() override;

  private:
    void updateFileIndexToExisting(int increment);
    void rescanFiles();

    IDir* m_ptrDir{};
    QStringList m_filesInDir{};
    int m_currentFileIndex{0};

    QStringList m_filePattern{"*.jpg", "*.JPG", "*.png", "*.PNG"};
};
