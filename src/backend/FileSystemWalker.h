#pragma once

#include <QDir>
#include <QStringList>

class FileSystemWalker
{
  public:
    FileSystemWalker(const QString& file, QList<QByteArray>& fileTypes);

    QString getCurrentFile() const;
    QString getNextFile();
    QString getPrevFile();

  private:
    void updateFileIndexToExisting(ssize_t increment);
    void rescanFiles();

    QDir m_dir{};
    QStringList m_filesInDir{};
    ssize_t m_currentFileIndex{0};

    QStringList m_filePattern{"*.jpg", "*.JPG", "*.png", "*.PNG"};
};
