#pragma once

#include "IFileSystemWalker.h"

#include <QDir>
#include <QStringList>

class FileSystemWalker : public IFileSystemWalker
{
  public:
    FileSystemWalker(const QString& file, QList<QByteArray>& fileTypes);
    virtual ~FileSystemWalker() = default;

    QString getCurrentFile() const override;
    QString getNextFile() override;
    QString getPrevFile() override;

  private:
    void updateFileIndexToExisting(int increment);
    void rescanFiles();

    QDir m_dir{};
    QStringList m_filesInDir{};
    int m_currentFileIndex{0};

    QStringList m_filePattern{"*.jpg", "*.JPG", "*.png", "*.PNG"};
};
